#include "DazMaxExporter.h"

/*This method is not in use and is here for documentation purposes. Skinning is on the roadmap but a long way off.*/

//Not sure why but the Daz default average does not include the first axis, so we define our own.
#define OUR_DEFAULT_AVERAGE		DzBoneBinding::FIRST_AXIS | \
								DzBoneBinding::SECOND_AXIS | \
								DzBoneBinding::THIRD_AXIS | \
								DzBoneBinding::SCALE_MAP | \
								DzBoneBinding::USE_GENERAL_MAP

void MyDazExporter::addSkinningData(DzFigure* figure, MyMesh& myMesh)
{
	if(!DzSkinBinding::isSingleSkinFigure(figure))
	{
		ShowMessage("DazMaxBridge can only export single skinned figures.");
	}
	
	DzSkinBinding* skin = figure->getSkinBinding();
	if(skin != NULL)
	{
		DzBoneBindingListIterator boneBindings = skin->getBoneBindingIterator();
		while(boneBindings.hasNext())
		{
			DzBoneBinding* binding = boneBindings.next();

			//Daz supports setting individual weights for the three axes.
			//For now, we will have it create a general map combining all these and the scale, but may extend it
			//in the future if the rig in Max makes good use of the information
			//bool local = binding->hasLocalWeights();


			DzWeightMapPtr weights = binding->createAveragedGeneralMap( OUR_DEFAULT_AVERAGE );
			
			MySkinningWeights skinning;
			skinning.BoneName = binding->getBone()->getName();
			skinning.Weights.assign(weights->getWeights(), weights->getWeights() + weights->getNumWeights());

			myMesh.WeightMaps.push_back(skinning);
		}
	}
}

int	MyDazExporter::addSkeletonData(DzSkeleton* skeleton, MyScene* scene)
{
	int index;
	for(index = 0; index < scene->Skeletons.size(); index++)
	{
		if(scene->Skeletons[index]._sourceSkeleton == skeleton){
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

	scene->Skeletons.push_back(mySkeleton);

	return index;
}