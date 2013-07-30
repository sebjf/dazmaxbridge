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
	    int		PositionVertex1;
	    int		PositionVertex2;
	    int		PositionVertex3;
	    int		PositionVertex4;

	    int		TextureVerticex1;
	    int		TextureVerticex2;
	    int		TextureVerticex3;
	    int		TextureVerticex4;

	    int		MaterialId;

	    /*Note this is not a messagepack capable object - these are packed into a raw array
    };
    */

    /* Remember this comes from MessagePack in a raw byte array */
    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Face
    {
        [FieldOffset(sizeof(int) * 0)]
        public int PositionVertex1;
        [FieldOffset(sizeof(int) * 1)]
        public int PositionVertex2;
        [FieldOffset(sizeof(int) * 2)]
        public int PositionVertex3;
        [FieldOffset(sizeof(int) * 3)]
        public int PositionVertex4;

        [FieldOffset(sizeof(int) * 4)]
        public int TextureVertex1;
        [FieldOffset(sizeof(int) * 5)]
        public int TextureVertex2;
        [FieldOffset(sizeof(int) * 6)]
        public int TextureVertex3;
        [FieldOffset(sizeof(int) * 7)]
        public int TextureVertex4;

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

	    int		SkeletonIndex;

	    MSGPACK_DEFINE(NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials, SkeletonIndex);

	    vector<pair<int,QString>> _materialsToProcess;

	    MaxMesh()
	    {
		    SkeletonIndex = -1;
	    }
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
        public Dictionary<int, Material> Materials;

        [MessagePackMember(7)]
        public int SkeletonIndex;

        public Face[] TriangulatedFaces = null;
    }

    /*
    class MaxBone
    {
    public:
	    string	Name;

	    float	OriginX;
	    float	OriginY;
	    float	OriginZ;

	    float	EndpointX;
	    float	EndpointY;
	    float	EndpointZ;

	    float	Qx;
	    float	Qy;
	    float	Qz;
	    float	Qw;

	    MSGPACK_DEFINE(Name,OriginX,OriginY,OriginZ,EndpointX,EndpointY,EndpointZ,Qx,Qy,Qz,Qw);
    };
     */

    public class MaxBone
    {
        [MessagePackMember(0)]
        public string Name;

        [MessagePackMember(1)]
        public float OriginX;
        [MessagePackMember(2)]
        public float OriginY;
        [MessagePackMember(3)]
        public float OriginZ;

        [MessagePackMember(4)]
        public float Qx;
        [MessagePackMember(5)]
        public float Qy;
        [MessagePackMember(6)]
        public float Qz;
        [MessagePackMember(7)]
        public float Qw;
    }

    /*
    class MaxSkeleton
    {
    public:
	    vector<MaxBone>	Bones;

	    MSGPACK_DEFINE(Bones);

	    DzSkeleton* _sourceSkeleton;

    };
     */

    public class MaxSkeleton
    {
        [MessagePackMember(0)]
        public List<MaxBone> Bones;
    }

    /*  
    class MaxScene
    {
    public:
	    vector<MaxMesh>		Items;
	    vector<MaxSkeleton>	Skeletons;

	    MSGPACK_DEFINE(Items, Skeletons);
    };
    */

    public class MaxScene
    {
        [MessagePackMember(0)]
        public List<MaxMesh> Items;
        [MessagePackMember(1)]
        public List<MaxSkeleton> Skeletons;

    }

}