// Daz3dsMaxCharacterExporter.cpp : Defines the exported functions for the DLL application.
//
#include "Daz3dsMaxCharacterExporter.h"
#include "Types.h"

#define		VERTEX_SIZE_IN_BYTES			(sizeof(DzPnt3))
#define		FACE_SIZE_IN_BYTES				(sizeof(int) * 4)
#define		FACE_MATERIAL_ID_SIZE_IN_BYTES	(sizeof(int))


QString VecToString(DzVec3 v)
{
	return (QString::number(v.m_x) + QString(", ") + QString::number(v.m_y) + QString(", ") + QString::number(v.m_z) + QString(", ") + QString::number(v.m_w));
}

void    MyDazExporter::getDefaultOptions( DzFileIOSettings *options ) const
{
	
}

MaxMesh	processMesh(DzFacetMesh* mesh)
{
	MaxMesh myMesh;

	// Collect the vertices into a float array - each vertex is three floats so its a simple copy

	myMesh.NumVertices = mesh->getNumVertices();
	myMesh.VerticesLengthInBytes = myMesh.NumVertices * VERTEX_SIZE_IN_BYTES;
	myMesh.Vertices.size = myMesh.VerticesLengthInBytes;
	myMesh.Vertices.ptr = (char*)malloc(myMesh.VerticesLengthInBytes);
	memcpy((void*)myMesh.Vertices.ptr, mesh->getVerticesPtr(), myMesh.VerticesLengthInBytes);

	// Collect the faces into an int array - each facet contains a number of properties such as material indices so split them out

	myMesh.NumFaces = mesh->getNumFacets();
	myMesh.FacesLengthInBytes = myMesh.NumFaces * FACE_SIZE_IN_BYTES;
	myMesh.Faces.ptr = (char*)malloc(myMesh.FacesLengthInBytes);
	myMesh.Faces.size = myMesh.FacesLengthInBytes;

	myMesh.FaceMaterialIDs.ptr = (char*)malloc(myMesh.NumFaces * FACE_MATERIAL_ID_SIZE_IN_BYTES);
	myMesh.FaceMaterialIDs.size = myMesh.NumFaces * FACE_MATERIAL_ID_SIZE_IN_BYTES;

	DzFacet* facets = mesh->getFacetsPtr();
	for(int i = 0; i < myMesh.NumFaces; i++)
	{
		((int*)myMesh.Faces.ptr)[(i * 4) + 0] = facets[i].m_vertIdx[0];
		((int*)myMesh.Faces.ptr)[(i * 4) + 1] = facets[i].m_vertIdx[1];
		((int*)myMesh.Faces.ptr)[(i * 4) + 2] = facets[i].m_vertIdx[2];
		((int*)myMesh.Faces.ptr)[(i * 4) + 3] = facets[i].m_vertIdx[3];

		((int*)myMesh.FaceMaterialIDs.ptr)[i] = facets[i].m_materialIdx;
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

	DzVertexMesh* mesh = object->getCachedGeom();

	if(!mesh->inherits("DzFacetMesh"))
	{
		return DZ_INVALID_SELECTION_ERROR;
	}
	
	MaxMesh myMesh = processMesh((DzFacetMesh*)mesh);

	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, myMesh);
	myFile.write(sbuf.data(), sbuf.size());

	myFile.close();
		
	return DZ_NO_ERROR;
}