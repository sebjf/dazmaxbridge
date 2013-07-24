
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
	),
	
	fn Test
	=(
		myBridge.Test()
	),
	
	fn Load
	=(
		myBridge.LoadFromFile("E:\Daz3D\Scripting\f2.characterkit")
	),
	
	fn MakeMesh
	=(
		return myGeometryProcessor.MakeMesh()
	),

	fn MakeMaterial
	=(
		return myMaterialProcessor.CreateMultiMaterial()
	)
	
)

bridge = MaxBridge()
bridge.LoadDependencies()
bridge.Test()
bridge.Load()
m = bridge.MakeMesh()
m.material = bridge.MakeMaterial()