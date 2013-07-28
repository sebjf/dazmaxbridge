// Daz3dsMaxCharacterExporter.cpp : Defines the exported functions for the DLL application.
//
#include "Daz3dsMaxCharacterExporter.h"

#define		VERTEX_SIZE_IN_BYTES			(sizeof(DzPnt3))
#define		FACE_SIZE_IN_BYTES				(sizeof(Face))
#define		FACE_MATERIAL_ID_SIZE_IN_BYTES	(sizeof(int))
#define		FLOATS_PER_VERTEX				3


QString VecToString(DzVec3 v)
{
	return (QString::number(v.m_x) + QString(", ") + QString::number(v.m_y) + QString(", ") + QString::number(v.m_z) + QString(", ") + QString::number(v.m_w));
}

void    MyDazExporter::getDefaultOptions( DzFileIOSettings *options ) const
{
	
}

MaxMesh	MyDazExporter::getMesh(DzObject* obj)
{
	DzFacetMesh* mesh = (DzFacetMesh*)obj->getCachedGeom();

	MaxMesh myMesh;

	// Collect the vertices into a float array - each vertex is three floats so its a simple copy

	myMesh.NumVertices = mesh->getNumVertices();
	myMesh.Vertices.assign( (float*)mesh->getVerticesPtr(), (float*)mesh->getVerticesPtr() + (myMesh.NumVertices * FLOATS_PER_VERTEX) );

	DzMap* uvMap = mesh->getUVs();
	myMesh.NumTextureVertices = uvMap->getNumValues();
	switch(uvMap->getType())
	{
	case DzMap::FLOAT_MAP:
		myMesh.TextureVertices.assign( uvMap->getFloatArrayPtr(), uvMap->getFloatArrayPtr() + myMesh.NumTextureVertices );
		break;
	case DzMap::FLOAT2_MAP:
		myMesh.TextureVertices.assign( (float*)uvMap->getPnt2ArrayPtr(), (float*)uvMap->getPnt2ArrayPtr() + (myMesh.NumTextureVertices * 2) );
		break;
	case DzMap::FLOAT3_MAP:
		myMesh.TextureVertices.assign( (float*)uvMap->getPnt3ArrayPtr(), (float*)uvMap->getPnt3ArrayPtr() + (myMesh.NumTextureVertices * 3) );
		break;
	}

	// Collect the faces into an int array - each facet contains a number of properties such as material indices so split them out

	myMesh.NumFaces = mesh->getNumFacets();
	myMesh.Faces.size = myMesh.NumFaces * FACE_SIZE_IN_BYTES;
	myMesh.Faces.ptr = (char*)malloc(myMesh.Faces.size);

	vector<int> materialsToProcess;

	Face* faces = (Face*)myMesh.Faces.ptr;

	DzFacet* facets = mesh->getFacetsPtr();
	for(int i = 0; i < myMesh.NumFaces; i++)
	{
		faces[i].PositionVertices[0] = facets[i].m_vertIdx[0];
		faces[i].PositionVertices[1] = facets[i].m_vertIdx[1];
		faces[i].PositionVertices[2] = facets[i].m_vertIdx[2];
		faces[i].PositionVertices[3] = facets[i].m_vertIdx[3];

		faces[i].TextureVertices[0] = facets[i].m_uvwIdx[0];
		faces[i].TextureVertices[1] = facets[i].m_uvwIdx[1];
		faces[i].TextureVertices[2] = facets[i].m_uvwIdx[2];
		faces[i].TextureVertices[3] = facets[i].m_uvwIdx[3];

		faces[i].MaterialId = facets[i].m_materialIdx;

		if(std::find(materialsToProcess.begin(),materialsToProcess.end(),facets[i].m_materialIdx) == materialsToProcess.end())
		{
			materialsToProcess.push_back(facets[i].m_materialIdx);
		}
	}


	DzShape* shape = obj->getCurrentShape();

	for(int i = 0; i < materialsToProcess.size(); i++)
	{
		Material myMaterial;

		myMaterial.MaterialIndex = materialsToProcess[i];
		
		DzMaterialFaceGroup* material_group = mesh->getMaterialGroup(myMaterial.MaterialIndex);
		myMaterial.MaterialName = material_group->getName();

		DzMaterial* material = shape->findMaterial(material_group->getName());

		if(material == NULL)
		{
			printf("Unable to find material.");
		}

		myMaterial.MaterialType = material->className();

		if(material->inherits("DzMaterial"))
		{
			myMaterial.MaterialProperties = getMaterialProperties((DzMaterial*)material);
		}

		myMesh.Materials.push_back(myMaterial);
	}

	return myMesh;
}

DzSkeleton* findBoneSkeleton(DzNode* node)
{
	DzNode* theParent = node->getNodeParent();
	if(theParent->inherits("DzSkeleton"))
	{
		return (DzSkeleton*)theParent;
	}
	return findBoneSkeleton(theParent);
}

MaxMesh MyDazExporter::getSkinnedFigure(DzObject* node)
{
	return getMesh(node); //for now we dont do skinning so just export the figure
}

MaxMesh MyDazExporter::getStaticMesh(DzObject* node)
{
	return getMesh(node);
}

void	MyDazExporter::resolveSelectedDzNode(DzNode* node)
{
	if(node->inherits("DzBone"))
	{
		//its a bone so find the parent skeleton and that will be the figure to export
		node = findBoneSkeleton(node);
	}

	DzObject* object = node->getObject();
	if(object != NULL)
	{
		//this node has geometry so its something we want to export
		if(object->getCachedGeom()->inherits("DzFacetMesh"))
		{
			if(find(processedNodes.begin(), processedNodes.end(), node) != processedNodes.end()){
				//if we have already encountered this node (which we might have done if the user has selected multiple bones in same skeleton for example), abort.
				return;
			}

			processedNodes.push_back(node);

			if(node->inherits("DzSkeleton"))
			{
				scene.Items.push_back( getSkinnedFigure(object) );
			}
			else
			{
				scene.Items.push_back( getStaticMesh(object) );
			}
		}
	}

	DzNodeListIterator nodeIterator = node->nodeChildrenIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node);		
	}
}

void	MyDazExporter::Reset()
{
	MaxScene newScene;
	scene = newScene;

	vector<DzNode*> newProcessedNodes;	
	processedNodes = newProcessedNodes;
}

DzError	MyDazExporter::write( const QString &filename, const DzFileIOSettings *options )
{
	Reset();

	DzNodeListIterator nodeIterator = dzScene->selectedNodeListIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node);		
	}

	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, scene);
	myFile.write(sbuf.data(), sbuf.size());

	myFile.close();
		
	return DZ_NO_ERROR;
}