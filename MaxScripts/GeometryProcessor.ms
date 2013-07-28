
struct	GeometryProcessor
(
	public
	
	myBridge = undefined,
	
	fn GetVertices
	=(
		local vertexCount= myBridge.GetNumVertices()
		local vertexData	= myBridge.GetVertices()
		
		local vertices = #()
		vertices[vertexCount] = 0
		
		for i = 0 to (vertexCount-1) do
		(
			vertices[i+1] = ( [ vertexData[(i * 3) + 1], vertexData[ (i * 3) + 2], vertexData[(i * 3) + 3] ] ) 
		)			
		
		return vertices
	),
	
	fn GetTextureVertices
	=(
		local vertexCount 	= myBridge.GetNumTextureVertices()
		local vertexData 	= myBridge.GetTextureVertices()
		
		local vertices = #()
		vertices[vertexCount] = 0
		
		local elementsPerVertex = vertexData.count / vertexCount;
		
		if(elementsPerVertex == 2)then
		(
			for i = 0 to (vertexCount-1) do
			(
				vertices[i+1] = ( [ vertexData[(i * 2) + 1], vertexData[ (i * 2) + 2], 0.0 ] ) 
			)
		)
		if(elementsPerVertex == 3)then
		(
			for i = 0 to (vertexCount-1) do
			(
				vertices[i+1] = ( [ vertexData[(i * 3) + 1], vertexData[ (i * 3) + 2], vertexData[(i * 3) + 3] ] ) 
			)
		)
		
		return vertices;
	),
	
	fn GetFaceData
	=(
		local faceCount			= myBridge.GetNumTriangulatedFaces()
		local faceData			= myBridge.GetTriangulatedFaces()
		
		local faceVertexIndices 	= #()
		local faceTextureIndices 	= #()
		local faceMaterials 			= #()
		
		progressUpdate 25.0
		
		local ptr = 1;
		
		faceVertexIndices[faceCount] 	= 0
		faceTextureIndices[faceCount] 	= 0
		faceMaterials[faceCount] 			= 0
		
		for i = 0 to ((faceCount-1)) do
		(
			fv1 = faceData[ptr + 0] + 1
			fv2 = faceData[ptr + 1] + 1
			fv3 = faceData[ptr + 2] + 1
			
			ft1 = faceData[ptr + 4] + 1
			ft2 = faceData[ptr + 5] + 1
			ft3 = faceData[ptr + 6] + 1
			
			fmid = faceData[ptr + 8] + 1;
			
			ptr = ptr + 9;
			
			faceVertexIndices[i+1] 	=	( [ fv1, fv2, fv3 ] )
			faceTextureIndices[i+1] 	=	( [ ft1, ft2, ft3 ] )
			faceMaterials[i+1] 			= 	fmid
		)			
		
		return #(faceVertexIndices, faceTextureIndices, faceMaterials)
	),
	
	
	fn MakeMesh
	=(
		progressStart "Creating mesh..."
		progressUpdate 0.0
		
		local myVertices = GetVertices()
		progressUpdate 10.0
		
		local myUVVertices = GetTextureVertices()
		progressUpdate 20.0
		
		local faceData = GetFaceData()
		progressUpdate 30.0
				
		local myMesh = mesh vertices:myVertices faces:faceData[1] materialIDs:faceData[3] tverts:myUVVertices
		progressUpdate 40.0
				
		buildTVFaces myMesh.mesh
		progressUpdate 50.0
		
		local faceTextureVertices = faceData[2]
		for i = 1 to myMesh.numFaces do
		(
			setTVFace myMesh.mesh i faceTextureVertices[i]
		)
		
		progressEnd()
		
		return myMesh
	)
	
	
)