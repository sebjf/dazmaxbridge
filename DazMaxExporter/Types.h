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

#include <QtCore\qstring.h>

using namespace msgpack;
using namespace std;

#define	BYTES				msgpack::type::raw_ref
#define MATERIALPROPERTIES	map<string,string>

#define		VERTEX_SIZE_IN_BYTES			(sizeof(DzPnt3))
#define		FACE_SIZE_IN_BYTES				(sizeof(Face))
#define		FACE_MATERIAL_ID_SIZE_IN_BYTES	(sizeof(int))
#define		FLOATS_PER_VERTEX				3

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
	int		PositionVertex1;
	int		PositionVertex2;
	int		PositionVertex3;
	int		PositionVertex4;

	int		TextureVertex1;
	int		TextureVertex2;
	int		TextureVertex3;
	int		TextureVertex4;

	int		MaterialId;

	/*Note this is not a messagepack capable object - these are packed into a raw array*/
};

class MyMesh
{
public:	
	string	Name;

	int		NumVertices;
	vector<float>	Vertices;

	int		NumTextureVertices;
	vector<float>	TextureVertices;

	int		NumFaces;
	BYTES	Faces;

	vector<Material> Materials;

	int		SkeletonIndex;

	MSGPACK_DEFINE(Name, NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials, SkeletonIndex);

	vector<pair<int,QString>> _materialsToProcess;

	MyMesh()
	{
		SkeletonIndex = -1;
	}
};

class MyBone
{
public:
	string	Name;

	float	OriginX;
	float	OriginY;
	float	OriginZ;

	float	Qx;
	float	Qy;
	float	Qz;
	float	Qw;

	MSGPACK_DEFINE(Name,OriginX,OriginY,OriginZ,Qx,Qy,Qz,Qw);
};

class DzSkeleton; //forward declaration for the cache helper member below

class MySkeleton
{
public:
	vector<MyBone>	Bones;

	MSGPACK_DEFINE(Bones);

	DzSkeleton* _sourceSkeleton;

};

class MyScene
{
public:
	vector<MyMesh>		Items;
	vector<MySkeleton>	Skeletons;

	MSGPACK_DEFINE(Items, Skeletons);
};

class MySceneItems
{
public:
	vector<string> Items;

	MSGPACK_DEFINE(Items);
};


#endif //MAXTYPES