using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MsgPack;
using MsgPack.Serialization;

namespace MaxBridgeLib
{
    public class Material
    {
        [MessagePackMember(0)]
	    public string MaterialName;
        [MessagePackMember(1)]
	    public int MaterialIndex;   //index as known by the mesh (i.e. the material slot)
        [MessagePackMember(2)]
        public string MaterialType;
        [MessagePackMember(3)]
        public Dictionary<string, string> MaterialProperties;
    }

    /*
    struct Face
    {
    public:
	    int		PositionVertices[4];
	    int		TextureVertices[4];
	    int		MaterialId;
    };
     */

    /* Remember this comes from MessagePack in a raw byte array */
    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Face
    {
        [FieldOffset(0)]
        public fixed int PositionVertices[4];
        [FieldOffset(sizeof(int) * 4)]
        public fixed int TextureVertices[4];
        [FieldOffset(sizeof(int) * 8)]
        public int MaterialId;       

    }

    /*
    class MaxMesh
    {
    public:	
	    int NumVertices;
	    vector<float>	Vertices;

	    int NumTextureVertices;
	    vector<float>	TextureVertices;

	    int		NumFaces;
	    BYTES	Faces;

	    map<int,Material> Materials;

	    MSGPACK_DEFINE(NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials);
    };
    */

    public class MaxMesh
    {
        [MessagePackMember(0)]
        public int NumVertices;
        [MessagePackMember(1)]
        public List<float> Vertices;

        [MessagePackMember(2)]
        public int NumTextureCoordinates;
        [MessagePackMember(3)]
        public List<float> TextureCoordinates;

        [MessagePackMember(4)]
        public int NumFaces;
        [MessagePackMember(5)]
        public byte[] Faces;

        [MessagePackMember(6)]
        public Dictionary<int,Material> Materials;

        public Face[] TriangulatedFaces = null;
    }

    /*  
    class MaxCollection
    {
    public:
	    vector<MaxMesh>	Items;

	    MSGPACK_DEFINE(Items);
    };
    */

    public class MaxScene
    {
        [MessagePackMember(0)]
        public List<MaxMesh> Items;
    }
}
