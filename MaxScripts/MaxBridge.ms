
struct MaxBridge
(	
	myBridge = undefined,
	myMaterialProcessor 	= undefined,
	mySkeletonProcessor 	= undefined,
	
	fn LoadDependencies
	=(
			local thisScriptPath = getFilenamePath (getThisScriptFilename())
			
			myBridge = DotNetObject "MaxManagedBridge.MaxPlugin"
			if(myBridge == undefined)then
			(
				MessageBox ("Could not load .NET library. Things to check: Autodesk.Max.Dll is installed, the library is targetting .NET Framework 3.5 Client.")
			)
			
			local StdMaterialProcessor 	= fileIn (thisScriptPath + "StdMaterialProcessor.ms")
			local SkeletonProcessor 	= fileIn (thisScriptPath + "SkeletonProcessor.ms")	
				
			myMaterialProcessor = StdMaterialProcessor 	myBridge:myBridge
			mySkeletonProcessor = SkeletonProcessor 	myBridge:myBridge
			
			if(heapSize < 300000000)then
			(
				heapSize += 300000000
			)
	),
	
	fn LoadFromFile
	=(
		myBridge.LoadFromFile "E:\Daz3D\Scripting\Scratch.characterkit" true
	),
	
	fn MakeMesh	myMesh
	=(
		maxMesh = mesh name:(myMesh.Name)
		
		myBridge.AddMeshData	(myBridge.Convert (GetHandleByAnim maxMesh)).ObjectRef.Mesh_ myMesh
		addmodifier maxMesh (smooth autoSmooth:true)
		
		return maxMesh
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
		
		disableSceneRedraw()
		suspendEditing()
		undo off(
			
			myMeshItems = myBridge.Scene.Items.ToArray()
			for i = 1 to myMeshItems.Count do
			(
				myMesh = myMeshItems[i]
				
				maxMesh = MakeMesh myMesh
				maxMesh.material = MakeMaterial myMesh 
				
				--myBridge.MakeNode 	myMesh
				
				if(myMesh.SkeletonIndex >= 0)then
				(
					-- do skeleton
					--mySkeletonProcessor.DoTestSkeleton	(myBridge.GetSkeletonIndex())
				)
			
			)
			
		)
		
		resumeEditing()
		enableSceneRedraw()
	)
	
)

bridge = MaxBridge()
bridge.LoadDependencies()
bridge.DoImport()