#ifndef DAZ_CHARACTER_EXPORTER
#define DAZ_CHARACTER_EXPORTER

#pragma warning (error: 4715) //Turns the warning 'not all control paths return a value' into an error.

#pragma warning(push)
#pragma warning( disable : 4005) //This is the only way I've been able to stop the winsock macro redifintion warning from appearing all other the place

#include "Types.h"

#pragma warning(pop)

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
	DzError				write(RequestParameters params, sharedmembuffer& sbuf);

private:

	void				resolveSelectedDzNode(DzNode* node, MyScene* collection);
	void				addFigure(DzFigure* skeleton, MyScene* collection);
	void				addNode(DzNode* node, MyScene* collection);

	void				addNodeData(DzNode* node, MyMesh& myMesh);
	void				addGeometryData(DzFacetMesh* dazMesh, MyMesh& myMesh);
	void				addMaterialData(DzNode* node, MyMesh& myMesh);
	void				addAnimationData(DzNode* node, MyMesh& myMesh, vector<DzTime> times);

	void				addSkinningData(DzFigure* figure, MyMesh& myMesh);
	int					addSkeletonData(DzSkeleton* skeleton, MyScene* collection);

	MATERIALPROPERTIES	getMaterialProperties(DzMaterial* material);

	/* Utilities */
	DzSkeletonList		getFigureFollowers(DzSkeleton* figure);
	DzShapeList			getFigureShapes(DzSkeletonList& figures);
	DzSkeleton*			findBoneSkeleton(DzNode* node);
	DzProperty*			findDzProperty(DzNode* node, QString name);
	DzProperty*			findDzProperty(DzPropertyGroup* group, QString name);
	int					setMeshResolution(DzNode* node, int newLevel);
	vector<DzTime>		getAnimationTimes(DzNode* node, AnimationType type);
	vector<DzTime>		getSceneKeyframeTimes();
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