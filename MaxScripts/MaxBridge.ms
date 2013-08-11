
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
	
	fn MakeMesh	myMesh
	=(
		--maxMesh = mesh name:(myMesh.Name)
		
		-- Get the node created above in '.net space', and send its 'mesh' member to the AddMeshData method for initialisation
		--myBridge.AddMeshData	(myBridge.Convert (GetHandleByAnim maxMesh)).ObjectRef.Mesh_ myMesh
		maxMesh = (GetAnimByHandle  (myBridge.Convert (myBridge.MakeMeshNode	myMesh)))
		addmodifier maxMesh (smooth autoSmooth:true)
		
		-- Swap the Z&Y axis by rotating the mesh by 90 deg. around X
		myXForm = xform()
		addmodifier maxMesh myXForm
		myXForm.center = [0,0,0]
		myXForm.gizmo.rotation = ((eulerangles -90 0 0) as quat)
		
		maxOps.CollapseNode maxMesh true
			
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
		
		-- Disable redraws/undo etc. for speed
		disableSceneRedraw()
		suspendEditing()
		undo off(
			
			myMeshItems = myBridge.Scene.Items.ToArray()
			for i = 1 to myMeshItems.Count do
			(
				-- myMesh here is a .NET object reference. We cant give it to native MaxScript functions, as they dont know what to do with it, but we can pass it around and access its properties
				myMesh = myMeshItems[i]
				
				maxMesh = MakeMesh myMesh
				maxMesh.material = MakeMaterial myMesh 
				
				if(myMesh.SkeletonIndex >= 0)then
				(
					-- implementation is not complete all this does at the moment is create spheres to indicate where bones would be for testing
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