#ifndef MAXTYPES
#define MAXTYPES

#include "msgpack.hpp"

using namespace msgpack;

#define	BYTES	msgpack::type::raw_ref

class Material
{
public:
	string				MaterialName;
	int					MaterialIndex;		//index as known by the mesh (i.e. the material slot)
	map<string,string>	MaterialProperties;

	MSGPACK_DEFINE(MaterialName, MaterialIndex, MaterialProperties);
};

class MaxMesh
{
public:	
	int		NumVertices;
	BYTES   Vertices;

	int		NumFaces;
	BYTES	Faces;
	BYTES	FaceMaterialIDs;

	vector<Material> Materials;

	MSGPACK_DEFINE(NumVertices, Vertices, NumFaces, Faces, FaceMaterialIDs, Materials);
};


#endif //MAXTYPES