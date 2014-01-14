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

void MyDazExporter::addNodeData(DzNode* node, MyMesh& myMesh)
{
	myMesh.Name = node->getLabel();
	if(myMesh.Name.size() <= 0){
		myMesh.Name = "Unnamed Object";
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
}

void MyDazExporter::addFigure(DzFigure* figure, MyScene* collection)
{
	if(sceneInfo.IsAGeograft(figure)){  //we dont export geografts so if its identified as one return
		return;
	}

	MyMesh myMesh;

	addNodeData(figure, myMesh);

	/* If headless make sure update() is called before getCachedGeom() */
	addGeometryData((DzFacetMesh*)(figure->getObject()->getCachedGeom()), myMesh);

	addMaterialData(figure, myMesh);

	DzSkeleton* parentSkeleton = figure->getFollowTarget();
	if(parentSkeleton == NULL)
	{
		parentSkeleton = figure;
	}
	else
	{
		myMesh.ParentName = parentSkeleton->getLabel(); //figures are parented to their follow targets, and these relationships override those of the scene hierarchy
	}

	myMesh.SkeletonIndex = addSkeletonData(parentSkeleton, collection);

	collection->Items.push_back( myMesh );

	/*Get the clothes (geografts will be filtered out by the statement at the top)*/
	/*(This loop processes siblings of the item, the one at the end of resolveSelectedDzNode() will process descendants)*/
	DzSkeletonList followers = sceneInfo.Followers[figure];
	for(int i = 0; i < followers.size(); i++)
	{
		resolveSelectedDzNode(followers[i], collection);
	}
}

void MyDazExporter::addNode(DzNode* node, MyScene* collection)
{
	MyMesh myMesh;

	addNodeData(node, myMesh);
	addGeometryData((DzFacetMesh*)(node->getObject()->getCachedGeom()), myMesh);
	addMaterialData(node, myMesh);

	collection->Items.push_back( myMesh );
}

void	MyDazExporter::resolveSelectedDzNode(DzNode* node, MyScene* collection)
{
	if(node->getObject() != NULL)
	{
		if(node->getObject()->getCachedGeom()->inherits("DzFacetMesh"))
		{
			if(node->inherits("DzFigure"))
			{
				addFigure((DzFigure*)node, collection);
			}
			else
			{
				addNode(node, collection);
			}
		}
	}

	DzNodeListIterator nodeIterator = node->nodeChildrenIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();
		resolveSelectedDzNode(node, collection);		
	}
}

DzError MyDazExporter::write(msgpack::sharedmembuffer& sbuf)
{
	return write(sceneInfo.TopLevelItemNames, sbuf);
}

DzError MyDazExporter::write(vector<string> labels, sharedmembuffer& sbuf)
{
	updateMySceneInformation();

	MyScene	sceneCollection;

	for(int i = 0; i < labels.size(); i++)
	{
		QString label = QString(labels[i].c_str());
		DzNode* start = dzScene->findNodeByLabel(label);
		if(start != NULL){
			resolveSelectedDzNode(start, &sceneCollection);
		}	
	}

	msgpack::pack(sbuf, sceneCollection);

	return DZ_NO_ERROR;
}

DzError MyDazExporter::write(const QString &filename)
{	
	QFile myFile(filename);
	myFile.open(QIODevice::ReadWrite | QIODevice::Truncate);

	msgpack::sharedmembuffer sbuf;
	write(sbuf);

	int written = myFile.write(sbuf.data(), sbuf.size());

	myFile.close();

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

void	MyDazExporter::updateMySceneInformation()
{
	sceneInfo = MySceneInformation();

	DzNodeListIterator nodeIterator = dzScene->nodeListIterator();
	while(nodeIterator.hasNext())
	{
		DzNode* node = nodeIterator.next();

		if(node->getNodeParent() != NULL)
		{
			continue; //only enumerate top level items
		}

		if(node->inherits("DzFigure"))
		{
			sceneInfo.Figures.push_back((DzFigure*)node);

			DzWeakFigureListIterator geograftIterator = ((DzFigure*)node)->graftFigureIterator();
			while(geograftIterator.hasNext()){
				DzFigure* geograft = geograftIterator.next();
				sceneInfo.GeograftList.push_back(geograft);
				sceneInfo.Geografts[((DzFigure*)node)].push_back(geograft);
			}
		}

		if(node->inherits("DzSkeleton"))
		{
			DzSkeleton* followTarget = ((DzSkeleton*)node)->getFollowTarget();
			if(followTarget != NULL){
				sceneInfo.Followers[followTarget].push_back((DzSkeleton*)node);
				continue; //if its a follower then its part of another 'asset' and we dont want to display it in the top level list
			}
		}

		if(node->getObject() != NULL)
		{
			if(node->getObject()->getCachedGeom()->inherits("DzFacetMesh"))
			{
				string label = node->getLabel();
				sceneInfo.TopLevelItemNames.push_back(label);
			}
		}
	}
}

bool MySceneInformation::IsAGeograft(DzNode* node)
{
	if(find(GeograftList.begin(), GeograftList.end(), node) != GeograftList.end()){	
		return true;
	}
	return false;
}