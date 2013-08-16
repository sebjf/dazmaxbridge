
struct MaxBridge
(	
	myBridge = undefined,
	
	--Check that the plugin has been loaded correctly (we can create an instance) and collect the scripts to process materials and rigs
	fn LoadDependencies
	=(			
			myBridge = DotNetObject "MaxManagedBridge.MaxPlugin"
			if(myBridge == undefined)then
			(
				MessageBox ("Could not load .NET library. Things to check: Autodesk.Max.Dll/Autodesk.Max.Dlu are present, the library is targetting .NET Framework 3.5 Client.")
			)			
	),
	
	fn LoadFromFile
	=(
		myBridge.LoadFromFile "E:\Daz3D\Scripting\Scratch.dazmaxbridge" true
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
			
		)
		resumeEditing()
		enableSceneRedraw()
	)
	
)

bridge = MaxBridge()
bridge.LoadDependencies()
bridge.DoImport()