filein	"CATManipulation.ms"

struct AutoRigger
(	
	---------------------------------------------------------------------
	-- Local Member Definitions
	---------------------------------------------------------------------
	
	CATManip,
	transSkinWeights,
	Initialised = false,
	
	---------------------------------------------------------------------
	-- The Constructor to intiailise CATManip tool (note this isnt a real constructor it must be called explicitly!)
	---------------------------------------------------------------------
	
	fn Constructor
	=(
		local s_CATManipulation = fileIn ("CATManipulation.ms")
		local s_transSkinWeights = fileIn ("transferSkinWeights.ms")
		CATManip = s_CATManipulation()
		transSkinWeights = s_transSkinWeights()
		return true
	),
	
	
	---------------------------------------------------------------------
	-- Helper Methods
	---------------------------------------------------------------------
	
	-- Returns an array of bones (Dummys) connected to a skinned mesh
	fn getSkinFromSkinnedMesh 		obj
	=(
		return (for m in obj.modifiers where (isKindOf m Skin) collect m)[1]
	),
	
	fn getBonesFromSkinnedMesh 	obj
	=(
		local theSkin 	 = getSkinFromSkinnedMesh 	obj
		local theBones = for o in objects where (refs.dependencyLoopTest theSkin o) collect o
		return theBones
	),
	

	-- Returns the first bone connected to a skinned mesh that matches the specified name
	fn getBoneFromSkinnedMesh 		obj  boneName
	=(
		local theBones = getBonesFromSkinnedMesh 	obj
		local theHip 	 = (for b in theBones where (b.name == boneName) collect b)[1]
		return theHip
	),
	

	-- Returns the first CAT Parent object in the scene
	fn getCATParent
	=(
		theCATParent = (for o in objects where (isKindOf o CATParent) collect o)[1]
	),
	

	---------------------------------------------------------------------
	-- The methods to align the bones
	---------------------------------------------------------------------
	
	-- Takes a single CAT bone and a set of Daz bones; finds an appropriate bone in the DAZBones and copies its transform to the CAT bone
	-- Mapped so this method can be called on an array of CATNodes
	mapped fn alignCATBoneToDAZBone	CATNode 	DAZBones
	=(
		local targetBone = (for b in DAZBones where (b.name == CATNode.name) collect b)[1]
		
		if(targetBone == undefined)do
		(
			format "Could not find a Daz Dummy match for %\n" CATNode.name
			return 0
		)
		
		if(targetBone.children.count <= 0)do
		(
			return 0
		)

		local dir = normalize (targetBone.transform.pos - targetBone.children[1].transform.pos)
		local tm = CATManip.getMatrixFromDir dir
					
		tm.pos = targetBone.transform.pos
			
		CATManip.setCATBoneTM CATNode tm true false
	),
	
	
	fn alignBones		DAZObj	CATObj
	=(
		
		-- Prepare the CAT rig for manipulation
		CATManip.prepareCATForManipulation 	CATObj
		
		-- Make the legs and arms very long.This solves an issue when the parent bone moves when 
		-- the child bone is moved if it is not sufficiently long.
		CATManip.prepareCATLimbsForManipulation 	CATObj
				
		-- Loop through all the CAT rig bones in the mapping list and align them.
		alignCATBoneToDAZBone	 CATObj.CATRigNodes (getBonesFromSkinnedMesh	DAZObj)
		
		transSkinWeights.swapSkinBones	DAZObj	CATObj reparseMapping:false
		
		return 0
	),
	
	---------------------------------------------------------------------
	-- Temporary methods to swap the bones in the skin
	---------------------------------------------------------------------
	fn swapBonesInSkin	DAZObj	CATObj
	=(
		skinMod = getSkinFromSkinnedMesh	DAZObj
		
		-- Make sure the skin modifier is selected in the modifier panel.
		max modify mode
		modPanel.setCurrentObject skinMod
		
		transSkinWeights.bakeMesh	DAZObj
		
		-- Turn off always deform in the skin modifier.
		skinMod.alwaysDeform = false
		
		
		
	),
	

	initialised = Constructor()
)

Rigger = AutoRigger()

DAZObj = $
CATObj = Rigger.getCATParent()

Rigger.alignBones 	DAZObj CATObj