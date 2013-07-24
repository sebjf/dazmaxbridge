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
	string				MaterialType;
	map<string,string>	MaterialProperties;

	MSGPACK_DEFINE(MaterialName, MaterialIndex, MaterialType, MaterialProperties);
};

struct Face
{
public:
	int		PositionVertices[4];
	int		TextureVertices[4];
	int		MaterialId;

	/*Note this is not a messagepack capable object - these are packed into a raw array*/
};

class MaxMesh
{
public:	
	int NumVertices;
	vector<float>	Vertices;

	int NumTextureVertices;
	vector<float>	TextureVertices;

	int		NumFaces;
	BYTES	Faces;

	vector<Material> Materials;

	MSGPACK_DEFINE(NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials);
};


#endif //MAXTYPES