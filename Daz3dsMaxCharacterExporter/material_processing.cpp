#include "Daz3dsMaxCharacterExporter.h"
#include "Types.h"
#include <sstream>

/*This method is responsible for turning a Daz material into a set of queryable properties 
of a consistent type.
It will convert all Daz Material property values into strings and store them in a map that
can be iterated in .NET*/

string PropertyToString(QString v)
{
	return v;
	QByteArray b = v.toUtf8();
	char* s_data = b.data();
	string s = string(s_data);
	return s;
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

map<string,string> MyDazExporter::getMaterialProperties(DzDefaultMaterial* material)
{
	map<string,string> properties;

	properties["allowsAutoBake"] = PropertyToString( material->allowsAutoBake() );
	properties["BaseOpacity"] = PropertyToString( material->getBaseOpacity() );
	properties["ColorMap"] = PropertyToString( material->getColorMap() );
	properties["DiffuseColor"] = PropertyToString( material->getDiffuseColor() );
	properties["MaterialType"] = PropertyToString( material->getMaterialName() );
	properties["NumGLMaps"] = PropertyToString( material->getNumGLMaps() );
	properties["OpacityMap"] = PropertyToString( material->getOpacityMap() );
	properties["isColorMappable"] = PropertyToString( material->isColorMappable() );
	properties["isOpacityMappable"] = PropertyToString( material->isOpacityMappable() );
	properties["isOpaque"] = PropertyToString( material->isOpaque() );
	properties["AmbientColor"] = PropertyToString( material->getAmbientColor() );
	properties["AmbientColorMap"] = PropertyToString( material->getAmbientColorMap() );
	properties["AmbientStrength"] = PropertyToString( material->getAmbientStrength() );
	properties["AmbientValueMap"] = PropertyToString( material->getAmbientValueMap() );
	properties["BumpMap"] = PropertyToString( material->getBumpMap() );
	properties["BumpMax"] = PropertyToString( material->getBumpMax() );
	properties["BumpMin"] = PropertyToString( material->getBumpMin() );
	properties["BumpStrength"] = PropertyToString( material->getBumpStrength() );
	properties["DiffuseStrength"] = PropertyToString( material->getDiffuseStrength() );
	properties["DiffuseValueMap"] = PropertyToString( material->getDiffuseValueMap() );
	properties["DisplacementMap"] = PropertyToString( material->getDisplacementMap() );
	properties["DisplacementMax"] = PropertyToString( material->getDisplacementMax() );
	properties["DisplacementMin"] = PropertyToString( material->getDisplacementMin() );
	properties["DisplacementStrength"] = PropertyToString( material->getDisplacementStrength() );
	properties["GlossinessStrength"] = PropertyToString( material->getGlossinessStrength() );
	properties["GlossinessValueMap"] = PropertyToString( material->getGlossinessValueMap() );
	properties["HorizontalOffset"] = PropertyToString( material->getHorizontalOffset() );
	properties["HorizontalTiles"] = PropertyToString( material->getHorizontalTiles() );
	properties["IndexOfRefraction"] = PropertyToString( material->getIndexOfRefraction() );
	properties["NormalValueMap"] = PropertyToString( material->getNormalValueMap() );
	properties["ReflectionColor"] = PropertyToString( material->getReflectionColor() );
	properties["ReflectionMap"] = PropertyToString( material->getReflectionMap() );
	properties["ReflectionStrength"] = PropertyToString( material->getReflectionStrength() );
	properties["ReflectionValueMap"] = PropertyToString( material->getReflectionValueMap() );
	properties["RefractionColor"] = PropertyToString( material->getRefractionColor() );
	properties["RefractionColorMap"] = PropertyToString( material->getRefractionColorMap() );
	properties["RefractionStrength"] = PropertyToString( material->getRefractionStrength() );
	properties["RefractionValueMap"] = PropertyToString( material->getRefractionValueMap() );
	properties["SpecularColor"] = PropertyToString( material->getSpecularColor() );
	properties["SpecularColorMap"] = PropertyToString( material->getSpecularColorMap() );
	properties["SpecularStrength"] = PropertyToString( material->getSpecularStrength() );
	properties["SpecularValueMap"] = PropertyToString( material->getSpecularValueMap() );
	properties["SurfaceType"] = PropertyToString( material->getSurfaceType() );
	properties["VerticalOffset"] = PropertyToString( material->getVerticalOffset() );
	properties["VerticalTiles"] = PropertyToString( material->getVerticalTiles() );
	properties["isMultThroughOpacity"] = PropertyToString( material->isMultThroughOpacity() );

	return properties;
}

