#ifndef MAXTYPES
#define MAXTYPES

 /*
 Note: it is important that _WINSOCKAPI_ is defined as a project wide preprocessor definition before
 including this header along with those of Daz and Qt 
 http://stackoverflow.com/questions/1372480/c-redefinition-header-files
 */

#pragma warning(push)
#pragma warning( disable : 4244)
#pragma warning( disable : 4267)

#include "msgpack.hpp"

#pragma warning(pop)


using namespace msgpack;
using namespace std;

#define	BYTES				msgpack::type::raw_ref
#define MATERIALPROPERTIES	map<string,string>

class Material
{
public:
	string				MaterialName;
	int					MaterialIndex;		//index as known by the mesh (i.e. the material slot)
	string				MaterialType;
	MATERIALPROPERTIES	MaterialProperties;

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

class MaxScene
{
public:
	vector<MaxMesh>	Items;

	MSGPACK_DEFINE(Items);
};

#endif //MAXTYPES