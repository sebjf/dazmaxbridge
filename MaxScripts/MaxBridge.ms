
struct MaxBridge
(	
	myBridge = undefined,
	myMaterialProcessor = undefined,
	myGeometryProcessor = undefined,
	mySkeletonProcessor = undefined,
	
	fn LoadDependencies
	=(
			local thisScriptPath = getFilenamePath (getThisScriptFilename())
			
			myBridge = DotNetObject "MaxManagedBridge.MaxPlugin"
			if(myBridge == undefined)then
			(
				MessageBox ("Could not load .NET library. Things to check: Autodesk.Max.Dll is installed, the library is targetting .NET Framework 3.5 Client.")
			)
			
			local MaterialProcessor = fileIn (thisScriptPath + "MaterialProcessor.ms")
			local GeometryProcessor = fileIn (thisScriptPath + "GeometryProcessor.ms")	
			local SkeletonProcessor = fileIn (thisScriptPath + "SkeletonProcessor.ms")	
				
			myMaterialProcessor = MaterialProcessor myBridge:myBridge
			myGeometryProcessor = GeometryProcessor myBridge:myBridge
			mySkeletonProcessor = SkeletonProcessor myBridge:myBridge
			
			if(heapSize < 300000000)then
			(
				heapSize += 300000000
			)
	),
	
	fn LoadFromFile
	=(
		myBridge.LoadFromFile "E:\Daz3D\Scripting\Scratch.characterkit" true
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
			
			myMeshItems = myBridge.Scene.Items.ToArray()
			for i = 1 to myMeshItems.Count do
			(
				myMesh = myMeshItems[i]
				
				maxMesh = mesh()
				myBridge.PopulateMesh	(myBridge.Convert (GetHandleByAnim maxMesh)).ObjectRef.Mesh_ myMesh
				
				--myBridge.MakeNode 	myMesh
				
				if(myMesh.SkeletonIndex >= 0)then
				(
					-- do skeleton
					--mySkeletonProcessor.DoTestSkeleton	(myBridge.GetSkeletonIndex())
				)
				
			--	mymesh.material = MakeMaterial()
			)
			
		)
		
		resumeEditing()
		enableSceneRedraw()
	)
	
)

bridge = MaxBridge()
bridge.LoadDependencies()
bridge.DoImport()