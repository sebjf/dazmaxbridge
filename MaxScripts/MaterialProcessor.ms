
struct	MaterialProcessor
(
	public
	
	myBridge = undefined,
		
	fn toColour		str
	=(
		myColourComponents = filterString str " "
		return (color (myColourComponents[2] as float * 255) (myColourComponents[3] as float * 255) (myColourComponents[4] as float * 255) (myColourComponents[1] as float * 255))
	),
	
	fn toPercent 	str
	=(
		return (str as integer * 100)
	),
	
	fn toMap		str
	=(
		return (bitmapTexture filename:str)
	),
	
	fn	DoUpdateMaterial		myMaterial	myProperty	myValue
	=(
		case	myProperty of(
			"BaseOpacity" 				: myMaterial.opacity = toPercent myValue
			"ColorMap"					: myMaterial.diffuseMap = toMap myValue
			"DiffuseColor"				: myMaterial.diffuse = toColour myValue
			"OpacityMap"				: myMaterial.opacitymap = toMap myValue
			"AmbientColor"				: myMaterial.ambient = toColour myValue
			"AmbientColorMap"		: myMaterial.ambientMap = toMap myValue
			"BumpMap"					: myMaterial.bumpMap = toMap myValue
			"SpecularColor"			: myMaterial.specular = toColour myValue
			"SpecularStrength"		: myMaterial.specularLevel = toPercent myValue
			"GlossinessStrength"		: myMaterial.glossiness = toPercent myValue
			
			
		)
		
	),
		
	fn GetStandardMaterial		myMaterialId
	=(
		myMaterial = StandardMaterial name:(myBridge.GetMaterialName myMaterialId)
		
		myMaterial.twoSided	 = true
		myMaterial.showInViewport = true
		myMaterial.adTextureLock = false
		myMaterial.adLock = false
		myMaterial.dsLock = false
		
		myMaterialProperties 	= myBridge.GetMaterialProperties 	myMaterialId
		myMaterialValues 		= myBridge.GetMaterialValues		myMaterialId
		
		if(myMaterialProperties.count <= 0)then
		(
			print "Warning: material " + myMaterialId + " of type " + (myBridge.GetMaterialType myMaterialId) + " came through with no properties."
		)
		
		for i = 1 to myMaterialProperties.count do
		(
			myProperty = myMaterialProperties[i];
			myValue = myMaterialValues[i];
			
			-- Place here whatever conversion functionality is desired. 
			
			-- For example, we could implement a mapper that uses 'execute' to build statements from a CSV, or pass the values back into .NET and have it derive the appropriate properties.			
			-- execute ("m" + ".opacity" + " = 50")
			
			-- For now though, we are going to use a big explicit switch statement, as this is slow, but also simple and allows us full control and visibility
			
			if(myValue.count > 0)then
			(
				DoUpdateMaterial		myMaterial myProperty MyValue
			)
		)
				
		return myMaterial
	),
	
	fn CreateMultiMaterial
	=(
		progressStart "Creating materials..."
		progressUpdate 0.0
		
		myMaterial = Multimaterial numsubs:(myBridge.GetHighestMaterialSlot() + 1)
		
		materialCount = myBridge.GetNumMaterials()
		for currentMaterialId = 0 to (materialCount-1) do
		(
			materialSlot 					= ((myBridge.GetMaterialSlot currentMaterialId) + 1)
			myMaterial[materialSlot] 	= (GetStandardMaterial currentMaterialId)
			progressUpdate (currentMaterialId * (100.0 / materialCount))
		)
		
		progressEnd()
		
		return myMaterial;
	)
	
	
)