#include "DazMaxExporter.h"
#include <sstream>

/*This method is responsible for turning a Daz material into a set of queryable properties 
of a consistent type.
It will convert all Daz Material property values into strings and store them in a map that
can be iterated in .NET*/

string PropertyToString(QString v)
{
	return string(v.toUtf8().data());
}

string PropertyToString(QColor& v)
{
	std::ostringstream s;
	s << v.alphaF() << " " << v.redF() << " " << v.greenF() << " " << v.blueF();
	return s.str();
}

string PropertyToString(int v)
{
	std::ostringstream s;
	s << v;
	return s.str();
}

string PropertyToString(double v)
{
	std::ostringstream s;
	s << v;
	return s.str();
}

string PropertyToString(DzTexture* v)
{
	if( v == NULL){
		return "";
	}
	return PropertyToString( v->getFilename() );
}

string PropertyToString(bool v)
{
	if(v) 
		return "True"; 
	else 
		return "False";
}

string PropertyToString(DzProperty* v)
{
	if(v->isA("DzImageProperty"))
	{
		return PropertyToString( ((DzImageProperty*)v)->getValue() );
	}
	if(v->isA("DzFloatProperty"))
	{
		return PropertyToString( ((DzFloatProperty*)v)->getRawValue() );
	}
	if(v->isA("DzColorProperty"))
	{
		return PropertyToString( ((DzColorProperty*)v)->getRawColorValue() );
	}
	if(v->isA("DzBoolProperty"))
	{
		return PropertyToString( ((DzBoolProperty*)v)->getRawBoolValue() );
	}
	if(v->isA("DzEnumProperty"))
	{
		return PropertyToString( ((DzEnumProperty*)v)->getRawStringValue() );
	}
	if(v->isA("DzStringProperty"))
	{
		return PropertyToString( ((DzStringProperty*)v)->getValue() );
	}
	if(v->isA("DzFileProperty"))
	{
		return PropertyToString( ((DzFileProperty*)v)->getPath() );
	}
	if(v->isA("DzIntProperty"))
	{
		return PropertyToString( ((DzIntProperty*)v)->getRawValue() );
	}
	return string((QString("Error: cannot resolve type ") + v->className() + QString(" to a string.")).toUtf8().data());
}

MATERIALPROPERTIES MyDazExporter::getMaterialProperties(DzMaterial* material)
{
	MATERIALPROPERTIES properties;

	//first, collect the color (diffuse) map along with other properties that Daz treats as 
	//special cases

	properties["Color Map"] = PropertyToString( material->getColorMap() );
	properties["Diffuse Color"] = PropertyToString( material->getDiffuseColor() );
	properties["Opacity Map"] = PropertyToString( material->getOpacityMap() );

	//this is also where the smoothing is done for when we are ready...

	/*	
	DzBoolProperty*	getSmoothControl () const
	virtual double 	getSmoothingAngle () const 
	*/

	//now, collect the arbitrary properties

	for( int i = 0; i < material->getNumProperties(); i++)
	{
		DzProperty* myProperty = material->getProperty(i);
		string propertyName = myProperty->getName().toUtf8().data();
		string propertyValue = PropertyToString(myProperty);
		properties[propertyName] = propertyValue;

		if(myProperty->inherits("DzNumericProperty"))
		{
			DzNumericProperty* myNumericProperty = ((DzNumericProperty*)myProperty);
			if(myNumericProperty->isMappable())
			{
				properties[ propertyName + " Map" ] = PropertyToString( myNumericProperty->getMapValue() );
			}
		}
	}

	return properties;

}

void	MyDazExporter::addMaterialData(DzNode* node, MyMesh& myMesh)
{
	DzShape* shape = node->getObject()->getCurrentShape();

	for(int i = 0; i < myMesh._materialsToProcess.size(); i++)
	{
		pair<int,QString>& materialToProcess = myMesh._materialsToProcess[i];

		Material myMaterial;

		myMaterial.MaterialIndex = materialToProcess.first;
		myMaterial.MaterialName  = materialToProcess.second;

		DzMaterial* material = shape->findMaterial(materialToProcess.second);
		
		if(material == NULL)
		{
			/*geografted geometry may result in faces in one shape referencing a group where the materials are actually defined in another, so here we try to find them*/
			DzSkeletonList geografts = sceneInfo.Geografts[node];
			for(DzSkeletonList::iterator itr = geografts.begin(); itr != geografts.end(); itr++)
			{
				QString geograftName = (*itr)->getLabel();
				if(materialToProcess.second.midRef(0, geograftName.length()).compare(geograftName) == 0)
				{
					QString materialName = materialToProcess.second.mid(geograftName.length() + 1);
					material = (*itr)->getObject()->getCurrentShape()->findMaterial(materialName);
					if(material != NULL){
						break;
					}
				}
			}
		}

		if(material == NULL)
		{
			QString message = QString("MyDazExporter: Unable to find material ") + QString(myMaterial.MaterialName.c_str());
			dzApp->statusLine(message);
			log.push_back(message);
		}
		else
		{
			myMaterial.MaterialType = material->className();

			if(material->inherits("DzMaterial"))
			{
				myMaterial.MaterialProperties = getMaterialProperties((DzMaterial*)material);
			}
		}

		/*Even if we know the material is a dud, add it anyway and force max to deal with it, notifying the user that something has gone wrong, instead of failing silently*/
		myMesh.Materials.push_back(myMaterial);
	}
}