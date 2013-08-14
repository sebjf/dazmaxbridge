
struct MaxBridge
(	
	myBridge = undefined,
	myMaterialProcessor 	= undefined,
	mySkeletonProcessor 	= undefined,
	
	--Check that the plugin has been loaded correctly (we can create an instance) and collect the scripts to process materials and rigs
	fn LoadDependencies
	=(
			local thisScriptPath = getFilenamePath (getThisScriptFilename())
			
			myBridge = DotNetObject "MaxManagedBridge.MaxPlugin"
			if(myBridge == undefined)then
			(
				MessageBox ("Could not load .NET library. Things to check: Autodesk.Max.Dll/Autodesk.Max.Dlu are present, the library is targetting .NET Framework 3.5 Client.")
			)
			
			local StdMaterialProcessor 	= fileIn (thisScriptPath + "StdMaterialProcessor.ms")
			local SkeletonProcessor 	= fileIn (thisScriptPath + "SkeletonProcessor.ms")	
			
			-- Replace the material and skeleton processors to change what materials and skinning systems are used (e.g. VRay, or BiPed or CAT)
			myMaterialProcessor = StdMaterialProcessor 	myBridge:myBridge
			mySkeletonProcessor = SkeletonProcessor 	myBridge:myBridge
			
			-- MaxScript controls its heap size using a similar algorithm to malloc(), which results in many allocations and consolidations when large amounts of data are involved. To speed things up we 'preallocate' a good size.
			if(heapSize < 300000000)then
			(
				heapSize += 300000000
			)
	),
	
	fn LoadFromFile
	=(
		myBridge.LoadFromFile "E:\Daz3D\Scripting\Scratch.dazmaxbridge" true
	),
	
	fn MakeMaterial	myMesh
	=(
		return myMaterialProcessor.CreateMultiMaterial		myMesh
	),
	
	fn DoImport
	=(
		if(myBridge == undefined)then
		(
			LoadDependencies()
		)
		LoadFromFile()
		
		-- Disable redraws/undo etc. for speed
		disableSceneRedraw()
		suspendEditing()
		undo off
		(
			--create all the meshes
			myBridge.UpdateAllMeshes()
			
			--and update the materials
			myMeshItems = myBridge.Scene.Items.ToArray()
			for m in myMeshItems do(
				myBridge.SetMeshMaterial	m	((GetHandleByAnim (MakeMaterial m)) as integer)
			)
						
		)
		resumeEditing()
		enableSceneRedraw()
	)
	
)

bridge = MaxBridge()
bridge.LoadDependencies()
bridge.DoImport()