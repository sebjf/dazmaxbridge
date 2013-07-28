#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#pragma warning (error: 4715)

#include "Types.h"

#include "dzapp.h"
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

using namespace std;

class PreparedFigure
{
public:
	DzFigure*		figure;
	DzObject*		object;
	DzSkeletonList followers;
};

/*Remember, if you make a mistake and Daz flips out it may revert to the beta workspace - change it in layout don't reinstall nothing is wrong!*/

class MyDazExporter : public DzExporter {
	Q_OBJECT
public:

	/** Constructor **/
	MyDazExporter() : DzExporter(QString("characterkit")) { }

public slots:

	virtual void    getDefaultOptions( DzFileIOSettings *options ) const;
	virtual QString getDescription() const { return QString("3DS Max Character Unwrapper"); };
	virtual bool	isFileExporter() const { return true; };

protected:

	virtual DzError	write( const QString &filename, const DzFileIOSettings *options );

private:

	MaxScene	scene;

	void				resolveSelectedDzNode(DzNode* node);
	void				addSkinnedFigure(DzFigure* node, DzObject* object);
	void				addStaticMesh(DzObject* object);

	void				addGeometryData(DzFacetMesh* dazMesh, MaxMesh& maxMesh);
	void				addMaterialData(DzShape* shape, DzShapeList shapes, MaxMesh& maxMesh);

	MaxMesh				getFigureMesh(DzObject* mesh, DzFigure* figure);
	MaxMesh				getMesh(DzObject* mesh);
	MATERIALPROPERTIES	getMaterialProperties(DzMaterial* material);

	/* Utilities */
	DzSkeletonList		getFigureFollowers(DzFigure* figure);
	DzShapeList			getFigureShapes(DzSkeletonList& figures);
	DzNodeList			getSkeletonBoneChildren(DzSkeleton* skeleton);
	DzSkeleton*			findBoneSkeleton(DzNode* node);

	/* Management & Reporting */
	vector<DzNode*> processedNodes;
	bool			hadErrors;

	void		Reset();

};

#endif // DAZ_CHARACTER_EXPORTER