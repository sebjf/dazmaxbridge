#include "Daz3dsMaxCharacterExporter.h"

int	MyDazExporter::addSkeletonData(DzSkeleton* skeleton)
{
	int index;
	for(index = 0; index < scene.Skeletons.size(); index++)
	{
		if(scene.Skeletons[index]._sourceSkeleton == skeleton){
			return index;
		}
	}

	DzBoneList bones;
	skeleton->getAllBones(bones);

	MaxSkeleton mySkeleton;

	for(int i = 0; i < bones.size(); i++)
	{
		MaxBone myBone;

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

	scene.Skeletons.push_back(mySkeleton);

	return index;
}