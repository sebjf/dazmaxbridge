// DazMaxExporter.cpp : Defines the exported functions for the DLL application.
//
#include "DazMaxExporter.h"

QString VecToString(DzVec3 v)
{
	return (QString::number(v.m_x) + QString(", ") + QString::number(v.m_y) + QString(", ") + QString::number(v.m_z) + QString(", ") + QString::number(v.m_w));
}

void ShowMessage(QString message)
{
	QMessageBox myMessageBox;
	myMessageBox.setText(message);
	myMessageBox.exec();
}

void MyDazExporter::addFigure(DzSkeleton* figure)
{
	if(find(sceneFigures.GeograftList.begin(), sceneFigures.GeograftList.end(), figure) != sceneFigures.GeograftList.end()){	//we dont export geografts so if its identified as one return
		return;
	}

	MyMesh myMesh;

	myMesh.Name = figure->getLabel();
	if(myMesh.Name.size() <= 0){
		myMesh.Name = "Unnamed Figure";
	}

	addGeometryData((DzFacetMesh*)(figure->getObject()->getCachedGeom()), myMesh);
	addMaterialData(figure->getObject()->getCurrentShape(), getFigureShapes(sceneFigures.Geografts[figure]), myMesh);

	DzSkeleton* parentSkeleton = figure->getFollowTarget();
	if(parentSkeleton == NULL)
	{
		parentSkeleton = figure;
	}
	else
	{
		myMesh.ParentName = parentSkeleton->getLabel(); //figures are only ever parented to their follow targets, unlike props
	}

	myMesh.SkeletonIndex = addSkeletonData(parentSkeleton);

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
	MyMesh myMesh;

	myMesh.Name = node->getLabel();
	if(myMesh.Name.size() <= 0){
		myMesh.Name = "Unnamed Node";
	}

	DzNode* parent = node->getNodeParent();
	if(parent != NULL)
	{
		if(parent->inherits("DzBone"))
		{
			parent = findBoneSkeleton(parent);
		}

		myMesh.ParentName = parent->getLabel();
	}

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
	scene = MyScene();
	addedNodes = vector<DzNode*>();
	log = vector<QString>();
	sceneFigures = SceneFigureInformation();
}

DzError MyDazExporter::write(msgpack::sbuffer& sbuf, const DzFileIOSettings* options)
{
	Reset();
	populateSceneFigureInformation();

	DzNodeListIterator nodeIterator = dzScene->nodeListIterator();
	while(nodeIterator.hasNext()){
		resolveSelectedDzNode(nodeIterator.next());		
	}
	
	msgpack::pack(sbuf, scene);

	return DZ_NO_ERROR;
}

DzError MyDazExporter::write(QIODevice& device, const DzFileIOSettings* options)
{	
	msgpack::sbuffer sbuf;
	write(sbuf, options);

	int written = device.write(sbuf.data(), sbuf.size());

	if(written != sbuf.size())
	{
		log.push_back("Unable to write file (reason unknown but not all bytes were written)");
	}

	if(log.size() > 0)
	{
		QString message = "Export completed but with problems:\n";
		for(int i = 0; i < log.size(); i++){
			message += (log[i] + "\n");
		}
		QMessageBox myMessageBox;
		myMessageBox.setText(message);
		myMessageBox.exec();	
	}

	return DZ_NO_ERROR;
}

DzError	MyDazExporter::write( const QString &filename, const DzFileIOSettings *options )
{
	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	DzError result = write(myFile, (DzFileIOSettings*)0);

	myFile.close();

	return result;
}

DzSkeletonList MyDazExporter::getFigureFollowers(DzSkeleton* figure)
{
	DzSkeletonList followers;

	DzNodeList children;
	figure->getNodeChildren(children, true); //the iterator is better but in this case we want the hiearchy traversal

	for(int i = 0; i < children.count(); i++)
	{
		if(children[i]->inherits("DzSkeleton"))
		{
			if(((DzSkeleton*)children[i])->getFollowTarget() == figure)
			{
				followers.push_back((DzSkeleton*)children[i]);
			}
		}
	}

	return followers;
}

DzShapeList	MyDazExporter::getFigureShapes(DzSkeletonList& figures)
{
	DzShapeList shapes;

	for(DzSkeletonList::Iterator itr = figures.begin(); itr != figures.end(); itr++)
	{
		if((*itr)->getObject() != NULL)
		{
			DzShape* shape = (*itr)->getObject()->getCurrentShape();
			if(shape != NULL)
			{
				shapes.push_back(shape);
			}
		}
	}

	return shapes;

}

DzNodeList MyDazExporter::getSkeletonBoneChildren(DzSkeleton* skeleton)
{
	DzNodeList children;

	DzBoneList bones;
	skeleton->getAllBones(bones);
	for(DzBoneList::Iterator itr = bones.begin(); itr != bones.end(); itr++)
	{
		DzNodeListIterator childIterator = (*itr)->nodeChildrenIterator();
		while(childIterator.hasNext())
		{
			children.push_back(childIterator.next());
		}
	}

	return children;

}

//There is a clear path to an exception here but Daz say Bones are not ever meant to be parented to a non bone/skeleton
DzSkeleton* MyDazExporter::findBoneSkeleton(DzNode* node)
{
	DzNode* theParent = node->getNodeParent();
	if(theParent->inherits("DzSkeleton"))
	{
		return (DzSkeleton*)theParent;
	}
	return findBoneSkeleton(theParent);
}

bool	MyDazExporter::IsAlreadyAddedNode(DzNode* node)
{
	if(find(addedNodes.begin(), addedNodes.end(), node) != addedNodes.end()){
		return true;
	}else{
		return false;
	}
}

void	MyDazExporter::populateSceneFigureInformation()
{
	DzSkeletonListIterator skeletonIterator = dzScene->skeletonListIterator();
	while(skeletonIterator.hasNext())
	{
		DzSkeleton* skeleton = skeletonIterator.next();

		DzSkeleton* followTarget = skeleton->getFollowTarget();
		if(followTarget != NULL){
			sceneFigures.Followers[followTarget].push_back(skeleton);
		}

		if(skeleton->inherits("DzFigure"))
		{
			DzFigure* figure = (DzFigure*)skeleton;

			sceneFigures.Figures.push_back(figure);

			DzWeakFigureListIterator geograftIterator = figure->graftFigureIterator();
			while(geograftIterator.hasNext()){
				DzFigure* geograft = geograftIterator.next();
				sceneFigures.GeograftList.push_back(geograft);
				sceneFigures.Geografts[figure].push_back(geograft);
			}

		}
	}
}