
struct	MaterialProcessor
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
				
	fn getDiffuseMap	myMaterial
	=(
		if(myMaterial.diffuseMap == undefined)then
		(
			myMaterial.diffuseMap = RGB_Multiply()
		)
		return myMaterial.diffuseMap
	),
	
	fn findBumpMapAmount myMaterialId
	=(
		return (((myBridge.TryGetMaterialProperty myMaterialId "Positive Bump") as float) - ((myBridge.TryGetMaterialProperty myMaterialId "Negative Bump") as float)) * ((myBridge.TryGetMaterialProperty myMaterialId "Bump Strength") as float) * 1000
	),
			
	fn	DoUpdateMaterial		myMaterial myMaterialId myProperty myValue
	=(
		case	myProperty of(
			"Ambient Color"			: myMaterial.ambient 						= toColour myValue
			"Ambient Color Map"		: myMaterial.ambientMap					= toMap myValue
			"Ambient Strength"		: myMaterial.ambientMapAmount 		= toPercent myValue
			"Bump Strength Map"	: myMaterial.bumpMap 						= toMap myValue
			"Color Map"					: (getDiffuseMap myMaterial).map1 	= toMap myValue
			"Diffuse Color"				: (getDiffuseMap myMaterial).color2 	= toColour myValue
			"Diffuse Strength"		: myMaterial.diffuseMapAmount 			= toPercent myValue
			"Opacity Map"				: myMaterial.opacityMap 					= toMap myValue
			"Opacity Strength"		: myMaterial.opacityMapAmount			= toPercent myValue
			"Specular Color"			: myMaterial.specular 						= toColour myValue
			"Specular Color Map"		: myMaterial.specularMap					= toMap myValue
			"Specular Strength"		: myMaterial.specularLevel 				= toPercent myValue
			"Glossiness"					: myMaterial.glossiness 						= toPercent myValue
		)
		
	),
		
	fn GetStandardMaterial		myMaterialId
	=(
		local myMaterial = StandardMaterial name:(myBridge.GetMaterialName myMaterialId)
		
		myMaterial.twoSided	 = true
		myMaterial.showInViewport = true
		myMaterial.adTextureLock = false
		myMaterial.adLock = false
		myMaterial.dsLock = false
		
		local myMaterialProperties 	= myBridge.GetMaterialProperties 	myMaterialId
		local myMaterialValues 		= myBridge.GetMaterialValues		myMaterialId
		
		if(myMaterialProperties.count <= 0)then
		(
			print "Warning: material " + (myMaterialId as string) + " of type " + (myBridge.GetMaterialType myMaterialId) + " came through with no properties."
			return myMaterial;
		)
				
		for i = 1 to myMaterialProperties.count do
		(
			local myProperty = myMaterialProperties[i];
			local myValue = myMaterialValues[i];
			
			-- Place here whatever conversion functionality is desired. 
			
			-- For example, we could implement a mapper that uses 'execute' to build statements from a CSV, or pass the values back into .NET and have it derive the appropriate properties.			
			-- execute ("m" + ".opacity" + " = 50")
			
			-- For now though, we are going to use a big explicit switch statement, as this is simple to understand and strikes a good balance between clarity and speed
			
			if(myValue.count > 0)then
			(
				DoUpdateMaterial		myMaterial myMaterialId myProperty MyValue
			)
		)
		
		try(
			myMaterial.bumpMapAmount = (findBumpMapAmount myMaterialId)
		)catch
		(
		)
				
		return myMaterial
	),
	
	fn CreateMultiMaterial
	=(
		progressStart "Creating materials..."
		progressUpdate 0.0
		
		local myMaterial = Multimaterial numsubs:(myBridge.GetHighestMaterialSlot() + 1)
		
		local materialCount = myBridge.GetNumMaterials()
		for currentMaterialId = 0 to (materialCount-1) do
		(
			local materialSlot 			= ((myBridge.GetMaterialSlot currentMaterialId) + 1)
			myMaterial[materialSlot] 	= (GetStandardMaterial currentMaterialId)
			progressUpdate (currentMaterialId * (100.0 / materialCount))
		)
		
		progressEnd()
		
		return myMaterial;
	)
	
	
)