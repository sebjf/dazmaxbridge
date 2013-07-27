// Daz3dsMaxCharacterExporter.cpp : Defines the exported functions for the DLL application.
//
#include "Daz3dsMaxCharacterExporter.h"
#include "Types.h"

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

		if(material->inherits("DzDefaultMaterial"))
		{
			//now store the material properties
			myMaterial.MaterialProperties = getMaterialProperties((DzDefaultMaterial*)material);
		}

		int propertyCount = material->getNumProperties();

		vector<QString> propertyNames;
		vector<QString> propertyTypes;

		for(int i = 0; i < propertyCount; i++)
		{
			DzProperty* prop = material->getProperty(i);

			OutputDebugString(prop->getName());
			OutputDebugString(prop->metaObject()->className());
			OutputDebugString("\n\n");

		}

		if(material->inherits("DzShaderMaterial"))
		{
			const QMetaObject* mo = material->metaObject();
			int mc = mo->methodCount();
			vector<string> methodNames;
			for(int i = mo->methodOffset(); i < mc; i++)
			{
				methodNames.push_back(string( mo->method(i).signature()));
			}
		}

		myMesh.Materials.push_back(myMaterial);
	}

	return myMesh;
}

DzError	MyDazExporter::write( const QString &filename, const DzFileIOSettings *options )
{
	DzNode* selection = dzScene->getPrimarySelection();
	DzSkeleton* skeleton = selection->getSkeleton();

	DzObject* object = selection->getObject();

	if( object == NULL )
	{
		return DZ_INVALID_SELECTION_ERROR;
	}

	if(!object->getCachedGeom()->inherits("DzFacetMesh"))
	{
		return DZ_INVALID_SELECTION_ERROR;
	}
	
	MaxMesh myMesh = getMesh(object);

	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, myMesh);
	myFile.write(sbuf.data(), sbuf.size());

	myFile.close();
		
	return DZ_NO_ERROR;
}