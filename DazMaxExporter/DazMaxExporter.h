#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#pragma warning (error: 4715)

#include "Types.h"
#include "MemoryMappedFiles.h"

#include <QtCore\qfile.h>
#include <QtCore\qmetaobject.h>
#include <QtCore\qcoreapplication.h>

using namespace std;

void ShowMessage(QString message);


/*Remember, if you make a mistake and Daz flips out it may revert to the beta workspace - change it in layout don't reinstall nothing is wrong!*/

class MyDazExporter : public QObject {
	Q_OBJECT
public:

	DzError		write(msgpack::sharedmembuffer& sbuf);
	DzError		write(vector<string> labels, sharedmembuffer& sbuf);
	DzError		write( const QString &filename );

private:

	void				resolveSelectedDzNode(DzNode* node, MyScene* collection);
	void				addFigure(DzFigure* skeleton, MyScene* collection);
	void				addNode(DzNode* node, MyScene* collection);

	void				addNodeData(DzNode* node, MyMesh& myMesh);
	void				addGeometryData(DzFacetMesh* dazMesh, MyMesh& myMesh);
	void				addMaterialData(DzNode* node, MyMesh& myMesh);
	void				addAnimationData(DzNode* node, MyMesh& myMesh);

	void				addBoneWeights(DzFigure* figure, MyMesh& myMesh);
	int					addSkeletonData(DzSkeleton* skeleton, MyScene* collection);

	MATERIALPROPERTIES	getMaterialProperties(DzMaterial* material);

	/* Utilities */
	DzSkeletonList		getFigureFollowers(DzSkeleton* figure);
	DzShapeList			getFigureShapes(DzSkeletonList& figures);
	DzSkeleton*			findBoneSkeleton(DzNode* node);
	vector<DzTime>		getKeyframeTimes(DzNode* node);
	void				getKeyframeTimes(DzNode* node, vector<DzTime>& times);
	void				getKeyframeTimes(DzObject* object, vector<DzTime>& times);
	void				getKeyframeTimes(DzFloatProperty* channel, vector<DzTime>& times);


public:
	void				updateMySceneInformation();
	MySceneInformation	sceneInfo;

private:	
	vector<QString> log;

	LARGE_INTEGER frequency;
	LARGE_INTEGER endTime;	
	LARGE_INTEGER startTime;

public:
	void startProfiling()
	{
		QueryPerformanceFrequency(&frequency);
		QueryPerformanceCounter(&startTime);
	}

	void endProfiling(QString message)
	{
		QueryPerformanceCounter(&endTime);
		dzApp->statusLine(message + QString::number(double(endTime.QuadPart-startTime.QuadPart)/double(frequency.QuadPart)) + " seconds.");
	}

};




#endif // DAZ_CHARACTER_EXPORTER