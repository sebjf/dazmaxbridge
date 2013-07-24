
struct	GeometryProcessor
(
	public
	
	myBridge = undefined,
	
	fn GetVertices
	=(
		vertexCount= myBridge.GetNumVertices()
		vertexData	= myBridge.GetVertices()
		
		vertices = #()
		
		for i = 0 to (vertexCount-1) do
		(
			append vertices ( [ vertexData[(i * 3) + 1], vertexData[ (i * 3) + 2], vertexData[(i * 3) + 3] ] ) 
		)			
		
		return vertices
	),
	
	fn GetTextureVertices
	=(
		vertexCount 	= myBridge.GetNumTextureVertices()
		vertexData 	= myBridge.GetTextureVertices()
		
		vertices = #()
		
		elementsPerVertex = vertexData.count / vertexCount;
		
		if(elementsPerVertex == 2)then
		(
			for i = 0 to (vertexCount-1) do
			(
				append vertices ( [ vertexData[(i * 2) + 1], vertexData[ (i * 2) + 2], 0.0 ] ) 
			)
		)
		if(elementsPerVertex == 3)then
		(
			for i = 0 to (vertexCount-1) do
			(
				append vertices ( [ vertexData[(i * 3) + 1], vertexData[ (i * 3) + 2], vertexData[(i * 3) + 3] ] ) 
			)
		)
		
		return vertices;
	),
	
	fn GetFaceData
	=(
		local faceCount			= myBridge.GetNumFaces()
		local faceData			= myBridge.GetFaces()
		
		faceVertexIndices = #()
		faceTextureIndices = #()
		faceMaterials 		= #()
		
		local ptr = 1;
		
		for i = 0 to (faceCount-1) do
		(
			fv1 = faceData[ptr + 0] + 1
			fv2 = faceData[ptr + 1] + 1
			fv3 = faceData[ptr + 2] + 1
			fv4 = faceData[ptr + 3] + 1
			
			ft1 = faceData[ptr + 4] + 1
			ft2 = faceData[ptr + 5] + 1
			ft3 = faceData[ptr + 6] + 1
			ft4 = faceData[ptr + 7] + 1 
			
			fmid = faceData[ptr + 8] + 1;
			
			ptr = ptr + 9;
			
			append faceVertexIndices 		( [ fv1, fv2, fv3 ] )
			append faceTextureIndices 	( [ ft1, ft2, ft3 ] )
			append faceMaterials 			fmid
			
			if(fv4 > 0)do(
				append faceVertexIndices 		( [ fv1, fv3, fv4 ] ) 
				append faceTextureIndices 	( [ ft1, ft3, ft4 ] )
				append faceMaterials 			fmid
			)
		)			
		
		return #(faceVertexIndices, faceTextureIndices, faceMaterials)
	),
	
	
	fn MakeMesh
	=(
		progressStart "Creating mesh..."
		progressUpdate 0.0
		
		myVertices = GetVertices()
		progressUpdate 10.0
		
		myUVVertices = GetTextureVertices()
		progressUpdate 20.0
		
		faceData = GetFaceData()
		progressUpdate 30.0
				
		myMesh = mesh vertices:myVertices faces:faceData[1] materialIDs:faceData[3] tverts:myUVVertices
		progressUpdate 40.0
				
		buildTVFaces myMesh.mesh
		progressUpdate 50.0
		
		faceTextureVertices = faceData[2]
		for i = 1 to myMesh.numFaces do
		(
			setTVFace myMesh.mesh i faceTextureVertices[i]
		)
		
		progressEnd()
		
		return myMesh
	)
	
	
)