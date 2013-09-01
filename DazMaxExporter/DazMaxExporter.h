#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#pragma warning (error: 4715)

#include "Types.h"

#include <QtCore\qfile.h>
#include <QtCore\qmetaobject.h>
#include <QtCore\qcoreapplication.h>

using namespace std;

void ShowMessage(QString message);


/*Remember, if you make a mistake and Daz flips out it may revert to the beta workspace - change it in layout don't reinstall nothing is wrong!*/

class MyDazExporter : public QObject {
	Q_OBJECT
public:

	DzError		write(msgpack::sbuffer& sbuf);
	DzError		write(vector<string> labels, msgpack::sbuffer& sbuf);
	DzError		write( const QString &filename );

private:

	void				resolveSelectedDzNode(DzNode* node, MyScene* collection);
	void				addFigure(DzFigure* skeleton, MyScene* collection);
	void				addNode(DzNode* node, MyScene* collection);

	void				addNodeData(DzNode* node, MyMesh& myMesh);
	void				addGeometryData(DzFacetMesh* dazMesh, MyMesh& myMesh);
	void				addMaterialData(DzNode* node, MyMesh& myMesh);

	void				addBoneWeights(DzFigure* figure, MyMesh& myMesh);
	int					addSkeletonData(DzSkeleton* skeleton, MyScene* collection);

	MATERIALPROPERTIES	getMaterialProperties(DzMaterial* material);

	/* Utilities */
	DzSkeletonList		getFigureFollowers(DzSkeleton* figure);
	DzShapeList			getFigureShapes(DzSkeletonList& figures);
	DzSkeleton*			findBoneSkeleton(DzNode* node);

public:
	void				updateMySceneInformation();
	MySceneInformation	sceneInfo;

private:	
	vector<QString> log;

};

#endif // DAZ_CHARACTER_EXPORTER