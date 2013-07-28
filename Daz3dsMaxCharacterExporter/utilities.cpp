#include "Daz3dsMaxCharacterExporter.h"

DzSkeletonList MyDazExporter::getFigureFollowers(DzFigure* figure)
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

DzSkeleton* MyDazExporter::findBoneSkeleton(DzNode* node)
{
	DzNode* theParent = node->getNodeParent();
	if(theParent->inherits("DzSkeleton"))
	{
		return (DzSkeleton*)theParent;
	}
	return findBoneSkeleton(theParent);
}