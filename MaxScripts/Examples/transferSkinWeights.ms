--***************************************************************
--* transferSkinWeights.ms
--* This file is part of the Mixamo Auto CAT script.
--*
--* This file defines a struct to perform skin weight transfer
--* from the original bones to the CAT bones.
--*
--* Copyright Mixamo www.mixamo.com 2011
--* Concept by Mixamo, script created by Ofer Zelichover for Mixamo www.mixamo.com
--* 
--***************************************************************


(

	-- A struct to deal with transferring skin weights from a one set of bones to another.
	struct s_TransferSkinWeights (
		_nameMapping = undefined, 		-- A property to hold a s_NameMapping instance with the name mapping.
		srcBones = undefined, 			-- Holds the source bones objects
		destBones = undefined, 			-- Holds the destination bones objects
		srcBonesToKeep = undefined, 	-- Holds an array of source bones that don't have matching CAT bones, so we can keep them in the skin modifier.
		
		
		
		-- This function bakes the current mesh pose by making a snapshot of the 
		-- mesh, and placing it in a channel in a morpher modifier.
		fn bakeMesh meshObj =
		(
			-- Create a snapshot of the mesh into a temp mesh
			local tempMesh = snapshot meshObj
			
			-- Add a morpher modifier before the skin modifier
			local morpherMod = morpher()
			for i = 1 to meshObj.modifiers.count where isKindOf meshObj.modifiers[i] skin do (
				-- Add the modifier
				addModifier meshObj morpherMod before:i
				-- Exit the loop
				exit
			)
			
			-- Add the tempMesh to the first channel in the morpher.
			WM3_MC_BuildFromNode morpherMod 1 tempMesh
			
			-- Set the value of the first channel to 100%
			WM3_MC_SetValue morpherMod 1 100.
			
			-- Delete the temp mesh
			delete tempMesh
		),
		
		
		-- This function solves the issue that the default load envelopes in max
		-- opens a window for bone mappping.
		-- This function automatically "presses" the OK button in that window.
		fn loadEnvelopes skinMod filename =
		(
			-- Make the skin mode active in the modifier panel
			modPanel.setCurrentObject skinMod
			
			-- Make sure no previous dialogMonitor functions are still registered.
			DialogMonitorOPS.unRegisterNotification id:#mixamoLoadEnvelopes
			-- Define the dialogMonitor function.
			fn dialogNotification = 
			(
				WindowHandle = DialogMonitorOPS.GetWindowHandle()
				UIAccessor.SendMessageID WindowHandle #IDOK
			)
			-- Register the dialogMonitor function and enable monitoring.
			DialogMonitorOPS.RegisterNotification dialogNotification id:#mixamoLoadEnvelopes
			DialogMonitorOPS.Enabled = true
			
			-- Load the envelopes
			skinOps.loadEnvelope skinMod filename
			
			-- Un-Register the dialogMonitor function and disable monitoring.
			DialogMonitorOPS.Enabled = false
			DialogMonitorOPS.unRegisterNotification id:#mixamoLoadEnvelopes
		),
		
		
		
		-- Removes the bone objects in boneOjbs from the given skin modifier.
		fn removeOldBones skinMod boneObjs =
		(
			-- Make a temp array with the boneObjs object names.
			local boneNames = for b in boneObjs where (findItem srcBonesToKeep b == 0) collect b.name as name
			
			-- Get the number of bones in the skin modifier.
			local numBones = skinOps.GetNumberBones skinMod
			
			-- Loop through the bones in the skin modifier.
			for i = numBones to 1 by -1 do (
				-- Get the bone name of the i'th bone in the modifier.
				local boneName = (skinOps.GetBoneName skinMod i 0) as name
				
				-- If the name exist in the boneNames array, then remove this bone.
				if findItem boneNames boneName > 0 then
					skinOps.removebone skinMod i
			)
		),
		
		
		
		-- Replaces the bone names in the envelopes file from the names of
		-- the source bones (given in srcBones) with the destination bones (in destBones).
		fn replaceBoneNamesInEnvFile filename srcBones destBones =
		(
			-- Create arrays with the bone names from the bone objects.
			local srcNames = for b in srcBones collect b.name
			local destNames = for b in destBones collect b.name
			
			-- Open the file.
			local srcFile = openFile filename mode:"r"
			-- Create a new dest. file into which the new data will be written.
			local destFName = getFilenamePath filename + "~" + getFilenameFile filename + getFilenameType filename
			local destFile = createFile destFName
			
			local str, boneName, n
			
			if srcFile != undefined and destFile != undefined then (
				-- Loop though the file's lines and analyze them, one by one.
				while not (eof srcFile) do (
					-- Read the current line
					str = readLine srcFile
					
					-- Check if this line contains a bone name definition
					if matchPattern str pattern:"[boneName] *" then (
						-- Get the bone name
						boneName = subString str 12 -1
						
						-- Find the destination bone name
						n = findItem srcNames boneName
						
						-- If a matching dest. name was found, replace the old name with the new one.
						if n > 0 then (
							-- Update the string with the new name
							str = "[boneName] " + destNames[n]
						)
					)
					-- Write the string to the dest file.
					format "%\n" str to:destFile
				)
				
				-- Close the files
				close srcFile
				flush destFile
				close destFile
			)
			
			-- Return the new created file name
			destFName
		),
		
		
		
		-- Returns array of matching bones (mixamo bones with CAT bones) and bones that have no matching bone.
		fn getBonesLists skinMod CATParentObj reparseMapping:true =
		(
			-- Make sure the skin modifier is selected in the modifier panel.
			max modify mode
			modPanel.setCurrentObject skinMod
			
			-- Init the arrays
			srcBones = #()
			destBones = #()
			srcBonesToKeep = #()
			
			-- Parse the naming file.
			if reparseMapping then
				_nameMapping.parseFile()
			
			
			local numBones = skinOps.GetNumberBones skinMod
			for i = 1 to numBones do (
				local boneName = skinOps.GetBoneName skinMod i 0
				local srcObj = getNodeByName boneName
			--	local destObj = _nameMapping.getCATNode boneName CATParentObj
				local destObj = (for o in CATParentObj.catrignodes where (o.name == boneName) collect o)[1]
				
				if isValidNode srcObj and isValidNode destObj then (
					append srcBones srcObj
					append destBones destObj
				) else (
					append srcBonesToKeep srcObj
					format "No matching CAT bone for %\n" boneName
				)
			)
		),
		
		
		
		-- Reparent mixamo bones that didn't have a matching CAT bone.
		fn reparentNonMatchingBones =
		(
			-- Loop throug the keep bones array
			for b in srcBonesToKeep do (
				-- Find the bone's parent index in the srcBones array
				local n = findItem srcBones b.parent
				
				-- If the bone's parent is in the srcBones array, that means the parent was swapped
				-- with a CAT bone, so reparent the bone to the matching CAT.
				if n > 0 then (
format "Reparenting %: % --> %\n" b.name b.parent.name destBones[n].name
					b.parent = destBones[n]
				)
			)
		),
		
		
		
		-- Swaps the srcBones that are in the skin modifier with the destBones.
		-- srcBones and destBones are arrays of objects. They both must have the same number 
		-- of items, as the bones are replace by replacing the n'th source bone with the n'th dest bone.
		fn swapSkinBones meshObj CATParentObj nameMapping: reparseMapping:true =
		(
			if nameMapping != unsupplied then
				_nameMapping = nameMapping
			
			-- Get the skin modifier from the mesh object.
			local skinMod = undefined
			for m in meshObj.modifiers while skinMod == undefined where isKindOf m skin do
				skinMod = m
			
			-- If no skin modifier was found, exit.
			if skinMod == undefined then
				return false
			
			
			-- Get the srcBones and destBones arrays
			getBonesLists skinMod CATParentObj reparseMapping:reparseMapping
			
			-- Make sure the skin modifier is selected in the modifier panel.
			max modify mode
			modPanel.setCurrentObject skinMod
			
			
			-- First, save the current envelopes.
			local filename = sysInfo.tempDir + "~boneReplacer" + timeStamp() as string + ".envASCII"
			skinOps.saveEnvelopeAsASCII skinMod filename
			
			-- ... and replace the names of the bones in the envelopes file.
			local newFilename = replaceBoneNamesInEnvFile filename srcBones destBones
			
			-- Since we don't have the rig in T-pose, we need to bake the mesh in it's current position
			-- so when we assign new bones to the skin modifier, the skinning won't break.
			bakeMesh meshObj
			
			-- Turn off always deform in the skin modifier.
			skinMod.alwaysDeform = false
			
			-- Now, remove the source bones from the skin modifier
			removeOldBones skinMod srcBones
			
			-- ... and add the destination bones to the skin modifier.
			for b in destBones do
				skinOps.addbone skinMod b 0
			
			-- Lastly, load the envelopes from the file saved erlier.
			loadEnvelopes skinMod newFilename
			
			-- Turn always deform in the skin modifier back on.
			skinMod.alwaysDeform = true
			
			-- Reparent non matching bones
			reparentNonMatchingBones()
			
			-- Clean up
			gc light:true
			deleteFile filename
			deleteFile newFilename
			
			newFilename
		)
		
	) -- End of s_TransferSkinWeights struct
	

)