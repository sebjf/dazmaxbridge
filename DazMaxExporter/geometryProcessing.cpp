#include "DazMaxExporter.h"

void MyDazExporter::addGeometryData(DzFacetMesh* mesh, MyMesh& myMesh)
{
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
		faces[i].PositionVertex1 = facets[i].m_vertIdx[0];
		faces[i].PositionVertex2 = facets[i].m_vertIdx[1];
		faces[i].PositionVertex3 = facets[i].m_vertIdx[2];
		faces[i].PositionVertex4 = facets[i].m_vertIdx[3];

		faces[i].TextureVertex1 = facets[i].m_uvwIdx[0];
		faces[i].TextureVertex2 = facets[i].m_uvwIdx[1];
		faces[i].TextureVertex3 = facets[i].m_uvwIdx[2];
		faces[i].TextureVertex4 = facets[i].m_uvwIdx[3];

		faces[i].MaterialId = facets[i].m_materialIdx;

		if(std::find(materialsToProcess.begin(),materialsToProcess.end(),facets[i].m_materialIdx) == materialsToProcess.end())
		{
			materialsToProcess.push_back(facets[i].m_materialIdx);
		}
	}

	//Before returning, create the materials, mapping indices to names so we can look up their properties later
	for(int i = 0; i < materialsToProcess.size(); i++)
	{
		DzMaterialFaceGroup* material_group = mesh->getMaterialGroup(materialsToProcess[i]);
		myMesh._materialsToProcess.push_back( pair<int,QString>( materialsToProcess[i], material_group->getName()) );
	}

}

int	MyDazExporter::setMeshResolution(DzNode* node, int newLevel)
{
	DzProperty* resolutionProperty = findDzProperty(node,"lodlevel");
	
	if(resolutionProperty == NULL)
	{
		return 0;
	}

	if(!resolutionProperty->inherits("DzIntProperty"))
	{
		return 0;
	}

	DzIntProperty* resolutionEnum = (DzIntProperty*)resolutionProperty;
	int oldLevel = resolutionEnum->getValue();

	if(newLevel != oldLevel)
	{
		resolutionEnum->setValue(newLevel);
		dzScene->update();
	}

	return oldLevel;
}