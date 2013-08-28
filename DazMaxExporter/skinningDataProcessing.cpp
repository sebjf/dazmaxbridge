#include "DazMaxExporter.h"

int	MyDazExporter::addSkeletonData(DzSkeleton* skeleton)
{
	int index;
	for(index = 0; index < scene.Skeletons.size(); index++)
	{
		if(scene.Skeletons[index]._sourceSkeleton == skeleton){
			return index;
		}
	}

	//if the skeleton exists then return the exact index. if it doesn't, create & add it and then return the index.

	DzBoneList bones;
	skeleton->getAllBones(bones);

	MySkeleton mySkeleton;

	for(int i = 0; i < bones.size(); i++)
	{
		MyBone myBone;

		DzBone*  bone = bones[i];

		myBone.Name = bone->getName();

		myBone.OriginX = bone->getWSPos().m_x;
		myBone.OriginY = bone->getWSPos().m_y;
		myBone.OriginZ = bone->getWSPos().m_z;

		myBone.Qx = bone->getWSRot().m_x;
		myBone.Qy = bone->getWSRot().m_y;
		myBone.Qz = bone->getWSRot().m_z;
		myBone.Qw = bone->getWSRot().m_w;

		mySkeleton.Bones.push_back(myBone);
	}

	mySkeleton._sourceSkeleton = skeleton;

	scene.Skeletons.push_back(mySkeleton);

	return index;
}