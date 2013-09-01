#ifndef MAXTYPES
#define MAXTYPES

#include "dzapp.h"
#include "dzaction.h"
#include <dzscene.h>
#include "dzexporter.h"
#include "dznode.h"
#include "dzskeleton.h"
#include "dzbone.h"
#include "dzfileio.h"
#include "dzfileiosettings.h"
#include "dzobject.h"
#include "dzvertexmesh.h"
#include "dzfacetshape.h"
#include "dzfacetmesh.h"
#include "dzmaterial.h"
#include "dzbasicmaterialarea.h"
#include "dzfacegroup.h"
#include "dzshaderbrick.h"
#include "dzshadermixerutility.h"
#include "dzdefaultmaterial.h"
#include "dztexture.h"
#include "dzmap.h"
#include "dzproperty.h"
#include "dzimageproperty.h"
#include "dzfloatproperty.h"
#include "dzcolorproperty.h"
#include "dzstringproperty.h"
#include "dzfileproperty.h"
#include "dzboolproperty.h"
#include "dzenumproperty.h"
#include "dzmodifier.h"
#include "dzfigure.h"
#include "dzshape.h"
#include "dzskin.h"
#include "dzskinbinding.h"

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

#define		FACE_SIZE_IN_BYTES				(sizeof(Face))
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
	string  ParentName;

	int		NumVertices;
	vector<float>	Vertices;

	int		NumTextureVertices;
	vector<float>	TextureVertices;

	int		NumFaces;
	BYTES	Faces;

	vector<Material> Materials;

	int		SkeletonIndex;
	

	MSGPACK_DEFINE(Name, ParentName, NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials, SkeletonIndex);

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

class MySceneInformation
{
public:
	vector<string>					TopLevelItemNames;

	vector<DzNode*>					Shapes;
	vector<DzFigure*>				Figures;
	vector<DzSkeleton*>				GeograftList;
	vector<DzSkeleton*>				FollowersList;
	map<DzNode*,DzSkeletonList>		Geografts;
	map<DzNode*,DzSkeletonList>		Followers;

	bool IsAGeograft(DzNode* node);

	MSGPACK_DEFINE(TopLevelItemNames);

};


#endif //MAXTYPES