#ifndef MAXTYPES
#define MAXTYPES

#include "msgpack.hpp"

using namespace msgpack;

#define	BYTES	msgpack::type::raw_ref

class MaxMesh
{
public:	
	int		NumVertices;
	int		VerticesLengthInBytes;
	BYTES   Vertices;

	int		NumFaces;
	int		FacesLengthInBytes;
	BYTES	Faces;
	BYTES	FaceMaterialIDs;

	MSGPACK_DEFINE(NumVertices, VerticesLengthInBytes, Vertices, NumFaces, FacesLengthInBytes, Faces, FaceMaterialIDs);
};


#endif //MAXTYPES