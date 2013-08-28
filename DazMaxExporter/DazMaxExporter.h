#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#pragma warning (error: 4715)

#include "Types.h"

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

#include <QtCore\qfile.h>
#include <QtCore\qmetaobject.h>
#include <QtCore\qcoreapplication.h>

using namespace std;

void ShowMessage(QString message);

class PreparedFigure
{
public:
	DzFigure*		figure;
	DzObject*		object;
	DzSkeletonList	followers;
};

class SceneFigureInformation
{
public:
	vector<DzFigure*>				Figures;
	vector<DzSkeleton*>				GeograftList;
	map<DzSkeleton*,DzSkeletonList> Geografts;
	map<DzSkeleton*,DzSkeletonList>	Followers;
};

/*Remember, if you make a mistake and Daz flips out it may revert to the beta workspace - change it in layout don't reinstall nothing is wrong!*/

class MyDazExporter : public QObject {
	Q_OBJECT
public:

	DzError		write(QIODevice& device, const DzFileIOSettings* options);
	DzError		write(msgpack::sbuffer& sbuf, const DzFileIOSettings* options);

protected:

	virtual DzError	write( const QString &filename, const DzFileIOSettings *options );

private:

	MyScene	scene;

	void				resolveSelectedDzNode(DzNode* node);
	void				addFigure(DzSkeleton* skeleton);
	void				addNode(DzNode* node);

	void				addGeometryData(DzFacetMesh* dazMesh, MyMesh& MyMesh);
	void				addMaterialData(DzShape* shape, DzShapeList shapes, MyMesh& MyMesh);
	int					addSkeletonData(DzSkeleton* skeleton);

	MATERIALPROPERTIES	getMaterialProperties(DzMaterial* material);

	/* Utilities */
	DzSkeletonList		getFigureFollowers(DzSkeleton* figure);
	DzShapeList			getFigureShapes(DzSkeletonList& figures);
	DzNodeList			getSkeletonBoneChildren(DzSkeleton* skeleton);
	DzSkeleton*			findBoneSkeleton(DzNode* node);

	void				populateSceneFigureInformation();

	/* Management & Reporting */
	SceneFigureInformation sceneFigures;
	vector<DzNode*> addedNodes;
	bool			IsAlreadyAddedNode(DzNode* node);
	
	vector<QString> log;

	void		Reset();

};

#endif // DAZ_CHARACTER_EXPORTER