
struct MaxBridge
(	
	myBridge = undefined,
	myMaterialProcessor = undefined,
	myGeometryProcessor = undefined,
	
	fn LoadDependencies
	=(
			local thisScriptPath = getFilenamePath (getThisScriptFilename())
			local	maxlib_dll = thisScriptPath + "MaxBridgeLib.dll"			
			local maxlib_assembly = dotnet.loadassembly maxlib_dll
			local maxlib_class = dotNetClass "MaxBridgeLib.MaxBridge"
			
			myBridge = dotNetObject maxlib_class
		
			if(myBridge == undefined)then
			(
				MessageBox ("Could not load " + maxlib_dll)
			)
			
			local MaterialProcessor = fileIn (thisScriptPath + "MaterialProcessor.ms")
			local GeometryProcessor = fileIn (thisScriptPath + "GeometryProcessor.ms")	
				
			myMaterialProcessor = MaterialProcessor myBridge:myBridge
			myGeometryProcessor = GeometryProcessor myBridge:myBridge
			
			if(heapSize < 300000000)then
			(
				heapSize += 300000000
			)
	),
	
	fn Test
	=(
		myBridge.Test()
	),
	
	fn LoadFromFile
	=(
		myBridge.LoadFromFile("E:\Daz3D\Scripting\f.characterkit")
	),
	
	fn MakeMesh
	=(
		return myGeometryProcessor.MakeMesh()
	),

	fn MakeMaterial
	=(
		return myMaterialProcessor.CreateMultiMaterial()
	),
	
	fn DoImport
	=(
		if(myBridge == undefined)then
		(
			LoadDependencies()
		)
		LoadFromFile()
		
		disableSceneRedraw()
		suspendEditing()
		undo off(
			
			sceneItemCount = myBridge.GetNumItems()
			
			for i = 1 to sceneItemCount do
			(
				myBridge.SetMesh (i-1)
				myMesh = MakeMesh()
				mymesh.material = MakeMaterial()
			)
			
		)
		
		resumeEditing()
		enableSceneRedraw()
	)
	
)

bridge = MaxBridge()
bridge.DoImport()