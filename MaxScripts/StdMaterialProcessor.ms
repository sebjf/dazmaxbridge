
struct	StdMaterialProcessor
(
	public
	
	myBridge = undefined,
	
	bumpAmountScalar = 1000,
		
	fn toColour		str
	=(
		local myColourComponents = filterString str " "
		return (color (myColourComponents[2] as float * 255) (myColourComponents[3] as float * 255) (myColourComponents[4] as float * 255) (myColourComponents[1] as float * 255))
	),
	
	fn toPercent 	str
	=(
		return (str as float * 100)
	),
	
	fn toMap		str
	=(
		return (bitmapTexture filename:str)
	),
		
	fn toFloat		str
	=(
		return (str as float)
	),
				
	fn getDiffuseMap	stdMaterial
	=(
		if(stdMaterial.diffuseMap == undefined)then
		(
			stdMaterial.diffuseMap = RGB_Multiply()
		)
		return stdMaterial.diffuseMap
	),
	
	fn findBumpMapAmount myMaterial
	=(
		return (((myMaterial.MaterialProperties.SafeGet "Positive Bump" "0") as float) - ((myMaterial.MaterialProperties.SafeGet "Negative Bump" "0") as float)) * ((myMaterial.MaterialProperties.SafeGet "Bump Strength" "0.03") as float) * 1000
	),
			
	fn	DoUpdateMaterial		stdMaterial myMaterial myProperty myValue
	=(
		case	myProperty of(
			"Ambient Color"			: stdMaterial.ambient 						= toColour myValue
			"Ambient Color Map"		: stdMaterial.ambientMap					= toMap myValue
			"Ambient Strength"		: stdMaterial.ambientMapAmount 		= toPercent myValue
			"Bump Strength Map"	: stdMaterial.bumpMap 						= toMap myValue
			"Color Map"					: (getDiffuseMap stdMaterial).map1 	= toMap myValue
			"Diffuse Color"				: (getDiffuseMap stdMaterial).color2 	= toColour myValue
			"Diffuse Strength"		: stdMaterial.diffuseMapAmount 			= toPercent myValue
			"Opacity Map"				: stdMaterial.opacityMap 					= toMap myValue
			"Opacity Strength"		: stdMaterial.opacityMapAmount			= toPercent myValue
			"Specular Color"			: stdMaterial.specular 						= toColour myValue
			"Specular Color Map"		: stdMaterial.specularMap					= toMap myValue
			"Specular Strength"		: stdMaterial.specularLevel 				= toPercent myValue
			"Glossiness"					: stdMaterial.glossiness 					= toPercent myValue
		)
		
	),
		
	fn GetStandardMaterial		myMaterial
	=(
		local stdMaterial = StandardMaterial name:(myMaterial.MaterialName)
		
		stdMaterial.twoSided	 = true
		stdMaterial.showInViewport = true
		stdMaterial.adTextureLock = false
		stdMaterial.adLock = false
		stdMaterial.dsLock = false
		
		local myMaterialProperties 	= myMaterial.MaterialProperties.KeysArray
		local myMaterialValues 		= myMaterial.MaterialProperties.ValuesArray
		
		if(myMaterialProperties.Count <= 0)then
		(
			print "Warning: material " + myMaterial.MaterialName + " of type " + myMaterial.MaterialType + " came through with no properties."
			return stdMaterial;
		)
				
		for i = 1 to myMaterialProperties.Count do
		(
			local myProperty = myMaterialProperties[i];
			local myValue = myMaterialValues[i];
			
			if(myValue.Count > 0)then --if the value is empty its equivalent to NULL so leave it as the default
			(
				DoUpdateMaterial		stdMaterial myMaterial myProperty MyValue
			)
		)
					
		stdMaterial.bumpMapAmount = (findBumpMapAmount myMaterial)
				
		return stdMaterial
	),
	
	fn CreateMultiMaterial	myMesh
	=(
		progressStart "Creating materials..."
		progressUpdate 0.0
		
		local maxMaterial = Multimaterial numsubs:(myMesh.NumberOfMaterialSlots)
		
		local myMaterials = myMesh.Materials.ToArray()
		local materialCount = myMaterials.Count
		for i = 1 to (materialCount) do
		(
			myMaterial = myMaterials[i]
			
			maxMaterial[(myMaterial.MaterialIndex + 1)] = (GetStandardMaterial myMaterial)
			
			progressUpdate (i * (100.0 / materialCount))
		)
		
		progressEnd()
		
		return maxMaterial;
	)
	
	
)