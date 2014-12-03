
#include "DazMaxExporter.h"

void MyDazExporter::addAnimationData(DzNode* node, MyMesh& myMesh)
{
	vector<DzTime> times = getKeyframeTimes(node);

	for(int i = 0; i < times.size(); i++)
	{
		dzScene->beginTimeEdit();
		dzScene->setTime(times[i]);
		dzScene->update();
		dzScene->finishTimeEdit();

		DzVertexMesh* mesh = node->getObject()->getCachedGeom();

		MyMeshKeyframe newKeyframe;
		newKeyframe.Name = node->name();
		newKeyframe.Time = (int)dzScene->getTime();
		newKeyframe.VertexPositions.assign( (float*)mesh->getVerticesPtr(), (float*)mesh->getVerticesPtr() + (myMesh.NumVertices * FLOATS_PER_VERTEX) );

		myMesh.Keyframes.push_back(newKeyframe);
	}

}

vector<DzTime> MyDazExporter::getKeyframeTimes(DzNode* node)
{
	vector<DzTime> times;
	getKeyframeTimes(node, times);
	sort(times.begin(), times.end());
	return times;
}

void MyDazExporter::getKeyframeTimes(DzNode* node, vector<DzTime>& times)
{
	getKeyframeTimes(node->getXPosControl(), times);
	getKeyframeTimes(node->getYPosControl(), times);
	getKeyframeTimes(node->getZPosControl(), times);
	getKeyframeTimes(node->getXRotControl(), times);
	getKeyframeTimes(node->getYRotControl(), times);
	getKeyframeTimes(node->getZRotControl(), times);
	getKeyframeTimes(node->getScaleControl(), times);
	getKeyframeTimes(node->getXScaleControl(), times);
	getKeyframeTimes(node->getYScaleControl(), times);
	getKeyframeTimes(node->getZScaleControl(), times);	
	getKeyframeTimes(node->getOriginXControl(), times);
	getKeyframeTimes(node->getOriginYControl(), times);
	getKeyframeTimes(node->getOriginZControl(), times);
	getKeyframeTimes(node->getEndXControl(), times);
	getKeyframeTimes(node->getEndYControl(), times);
	getKeyframeTimes(node->getEndZControl(), times);
	getKeyframeTimes(node->getOrientXControl(), times);
	getKeyframeTimes(node->getOrientYControl(), times);
	getKeyframeTimes(node->getOrientZControl(), times);
	//DzNumericNodeProperty*	getPointAtControl() const;

	DzObject* object = node->getObject();
	if(object != NULL)
	{
		getKeyframeTimes(object, times);
	}

	DzNodeListIterator childIterator = node->nodeChildrenIterator();
	while(childIterator.hasNext())
	{
		getKeyframeTimes(childIterator.next(), times);
	}

}

void MyDazExporter::getKeyframeTimes(DzObject* object, vector<DzTime>& times)
{
	DzModifierIterator iterator = object->modifierIterator();
	while(iterator.hasNext())
	{
		DzModifier* modifier = iterator.next();

		/* These are the three modifiers that have value channels which can be keyed. The skeletal 
		transforms are in the value channels directly on the nodes relating to their transforms */

		if(modifier->isA("DzDFormModifier")){
			getKeyframeTimes(((DzDFormModifier*)modifier)->getValueChannel(), times);
		}else
		if(modifier->isA("DzMorph")){
			getKeyframeTimes(((DzMorph*)modifier)->getValueChannel(), times);
		}else
		if(modifier->isA("DzPushModifier")){
			//getKeyframeTimes(((DzPushModifier*)modifier)->getValueChannel(), times); //Linker error for this function
		}

	}
}

void MyDazExporter::getKeyframeTimes(DzFloatProperty* channel, vector<DzTime>& times)
{
	for(int i = 0; i < channel->getNumKeys(); i++)
	{
		DzTime keyTime = channel->getKeyTime(i);
		if(std::find(times.begin(), times.end(), keyTime) == times.end())
		{
			times.push_back(keyTime);
		}
	}
}