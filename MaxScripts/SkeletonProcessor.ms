
struct	SkeletonProcessor
(
	public
	
	myBridge = undefined,
	
	fn drawLineBetweenTwoPoints pointA pointB
	=(
	  ss = SplineShape pos:pointA
	  addNewSpline ss
	  addKnot ss 1 #corner #line PointA
	  addKnot ss 1 #corner #line PointB
	  updateShape ss
	  ss
	),
	
	fn DoTestSkeleton	index
	=(
		local boneCount = (myBridge.GetNumBones index)
		
		for i = 0 to (boneCount-1) do
		(
			local transformData = (myBridge.GetBoneTransform index i)
			
			local Origin = ( [ transformData[1], transformData[2], transformData[3] ] )
			local Orientation = ( [ transformData[4], transformData[5], transformData[6], transformData[7] ] )
			
			--local ss = drawLineBetweenTwoPoints Origin Endpoint
			--ss.name = (myBridge.GetBoneName index i)
			
			 sphere radius:4 position:Origin name:(myBridge.GetBoneName index i)
		)
		
	)

	
)