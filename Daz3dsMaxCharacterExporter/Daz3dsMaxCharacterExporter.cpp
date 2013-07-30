// Daz3dsMaxCharacterExporter.cpp : Defines the exported functions for the DLL application.
//
#include "Daz3dsMaxCharacterExporter.h"


QString VecToString(DzVec3 v)
{
	return (QString::number(v.m_x) + QString(", ") + QString::number(v.m_y) + QString(", ") + QString::number(v.m_z) + QString(", ") + QString::number(v.m_w));
}

void    MyDazExporter::getDefaultOptions( DzFileIOSettings *options ) const
{
	
}

void MyDazExporter::addFigure(DzSkeleton* figure)
{
	if(find(sceneFigures.GeograftList.begin(), sceneFigures.GeograftList.end(), figure) != sceneFigures.GeograftList.end()){	//we dont export geografts so if its identified as one return
		return;
	}

	MaxMesh myMesh;
	addGeometryData((DzFacetMesh*)(figure->getObject()->getCachedGeom()), myMesh);
	addMaterialData(figure->getObject()->getCurrentShape(), getFigureShapes(sceneFigures.Geografts[figure]), myMesh);

	myMesh.SkeletonIndex = addSkeletonData(figure);

	scene.Items.push_back( myMesh );

	/*the main resolve method will not iterate over bones, so we do so here checking for things like props and hair (which we also want to be parented)*/
	DzNodeList children = getSkeletonBoneChildren(figure);
	for(int i = 0; i < children.size(); i++)
	{
		resolveSelectedDzNode(children[i]);
	}

	/*Get the clothes (geografts will be filtered out by the statement at the top)*/
	DzSkeletonList followers = sceneFigures.Followers[figure];
	for(int i = 0; i < followers.size(); i++)
	{
		resolveSelectedDzNode(followers[i]);
	}
}

void MyDazExporter::addNode(DzNode* node)
{
	MaxMesh myMesh;
	addGeometryData((DzFacetMesh*)(node->getObject()->getCachedGeom()), myMesh);
	addMaterialData(node->getObject()->getCurrentShape(), DzShapeList(), myMesh);
	scene.Items.push_back( myMesh );
}

void	MyDazExporter::resolveSelectedDzNode(DzNode* node)
{
	if(node->inherits("DzBone"))
	{
		//its a bone so find the parent skeleton and that will be the figure to export
		node = findBoneSkeleton(node);
	}

	if(IsAlreadyAddedNode(node)){
		return;
	}

	if(node->getObject() != NULL)
	{
		if(node->getObject()->getCachedGeom()->inherits("DzFacetMesh"))
		{
			addedNodes.push_back(node);

			if(node->inherits("DzSkeleton"))
			{
				addFigure((DzSkeleton*)node);
			}
			else
			{
				addNode(node);
			}
		}
	}

	DzNodeListIterator nodeIterator = node->nodeChildrenIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node);		
	}
}

void	MyDazExporter::Reset()
{
	scene = MaxScene();
	addedNodes = vector<DzNode*>();
	log = vector<QString>();
	sceneFigures = SceneFigureInformation();
}

DzError	MyDazExporter::write( const QString &filename, const DzFileIOSettings *options )
{
	Reset();
	populateSceneFigureInformation();

	DzNodeListIterator nodeIterator = dzScene->selectedNodeListIterator();
	while(nodeIterator.hasNext()){
		resolveSelectedDzNode(nodeIterator.next());		
	}

	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sbuffer sbuf;
	msgpack::pack(sbuf, scene);
	myFile.write(sbuf.data(), sbuf.size());

	myFile.close();

	if(log.size() > 0)
	{
		QMessageBox myMessageBox;
		QString message = "Export completed but with problems:\n";
		for(int i = 0; i < log.size(); i++){
			message += (log[i] + "\n");
		}
		myMessageBox.setText(message);
		myMessageBox.exec();	
	}
		
	return DZ_NO_ERROR;
}