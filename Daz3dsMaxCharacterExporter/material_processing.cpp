#include "Daz3dsMaxCharacterExporter.h"
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
		return PropertyToString( ((DzEnumProperty*)v)->getRawStringValue() );
	}
	if(v->isA("DzFileProperty"))
	{
		return PropertyToString( ((DzEnumProperty*)v)->getRawStringValue() );
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
