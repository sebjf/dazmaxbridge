#include "DazMaxExporter.h"


DzProperty* MyDazExporter::findDzProperty(DzPropertyGroup* group, QString name)
{
	if(group == NULL)
	{
		return NULL;
	}

	DzPropertyListIterator properties = group->getProperties();
	while(properties.hasNext())
	{
		DzProperty* prop = properties.next();
		if(QString::compare( prop->name(), name, Qt::CaseInsensitive) == 0)
		{
			return prop;
		}
	}

	DzProperty* prop = NULL;
	prop = findDzProperty( group->getFirstChild(), name);
	if(prop != NULL){
		return prop;
	}
	prop = findDzProperty( group->getNextSibling(), name);
	if(prop != NULL){
		return prop;
	}

	return NULL;
}

DzProperty*	MyDazExporter::findDzProperty(DzNode* node, QString name)
{
	DzPropertyGroupTree* propertyTree = node->getPropertyGroups();
	DzPropertyGroup* group = propertyTree->getFirstChild();
	return findDzProperty(group, name);
}