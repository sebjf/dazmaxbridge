using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using MsgPack;
using MsgPack.Serialization;
using System.Drawing;

namespace MaxManagedBridge
{
    /* The methods here allow the Dictionary to be used in Max (since MaxScript cannot invoke the operator overloads) */
    public class MaxDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public TValue Get(TKey key)
        {
            return this[key];
        }

        public TValue SafeGet(TKey key, TValue fallback)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return fallback;
            }
        }

        public TValue SafeGet(TKey key)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                return default(TValue);
            }
        }

        public TKey[] KeysArray
        {
            get { return this.Keys.ToArray(); }
        }

        public TValue[] ValuesArray
        {
            get { return this.Values.ToArray(); }
        }
    }

    /*
    class Material
    {
    public:
	    string				MaterialName;
	    int					MaterialIndex;		//index as known by the mesh (i.e. the material slot)
	    string				MaterialType;
	    MATERIALPROPERTIES	MaterialProperties;

	    MSGPACK_DEFINE(MaterialName, MaterialIndex, MaterialType, MaterialProperties);
    };
     */

    public class Material
    {
        [MessagePackMember(0)]
        public string MaterialName;
        [MessagePackMember(1)]
        public int MaterialIndex;   //index as known by the mesh (i.e. the material slot)
        [MessagePackMember(2)]
        public string MaterialType;
        [MessagePackMember(3)]
        public MaxDictionary<string, string> MaterialProperties;

        public Nullable<Color> GetColor(string key)
        {
            if (!MaterialProperties.ContainsKey(key))
                return null;

            string[] components = MaterialProperties[key].Split(' ');
            float a = float.Parse(components[0]);
            float r = float.Parse(components[1]);
            float g = float.Parse(components[2]);
            float b = float.Parse(components[3]);
            return Color.FromArgb((int)(a * 255f), (int)(r * 255f), (int)(g * 255f), (int)(b * 255f));
        }

        public Nullable<float> GetFloat(string key)
        {
            if (!MaterialProperties.ContainsKey(key))
                return null;

            return float.Parse(MaterialProperties[key]);
        }

        public string GetString(string key)
        {
            if (!MaterialProperties.ContainsKey(key))
                return null;

            string v = MaterialProperties[key];

            if (v.Length <= 0)
                return null;

            return v;
        }

        public float GetFloatSafe(string key, float fallback)
        {
            if(!MaterialProperties.ContainsKey(key))
                return fallback;
            return float.Parse(MaterialProperties[key]);
        }
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
    class MyMesh
    {
    public:	
	    string Name;

	    int NumVertices;
	    vector<float>	Vertices;

	    int NumTextureVertices;
	    vector<float>	TextureVertices;

	    int		NumFaces;
	    BYTES	Faces;

	    vector<Material> Materials;

	    int		SkeletonIndex;

	    MSGPACK_DEFINE(Name, NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials, SkeletonIndex);

	    vector<pair<int,QString>> _materialsToProcess;

	    MyMesh()
	    {
		    SkeletonIndex = -1;
	    }
    };
    */

    public class MyMesh
    {
        [MessagePackMember(0)]
        public string Name;
        [MessagePackMember(1)]
        public string ParentName;

        [MessagePackMember(2)]
        public int NumVertices;
        [MessagePackMember(3)]
        public List<float> Vertices;

        [MessagePackMember(4)]
        public int NumTextureCoordinates;
        [MessagePackMember(5)]
        public List<float> TextureCoordinates;

        [MessagePackMember(6)]
        public int NumFaces;
        [MessagePackMember(7)]
        public byte[] Faces;

        [MessagePackMember(8)]
        public List<Material> Materials;

        [MessagePackMember(9)]
        public int SkeletonIndex;

        /* The following properties are .NET only */

        public Face[] TriangulatedFaces = null;

        public int NumberOfMaterialSlots
        {
            get { return Materials.Max(Material => Material.MaterialIndex) + 1; }
        }

    }

    /*
    class MyBone
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

    public class MyBone
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
    class MySkeleton
    {
    public:
	    vector<MaxBone>	Bones;

	    MSGPACK_DEFINE(Bones);

	    DzSkeleton* _sourceSkeleton;

    };
     */

    public class MySkeleton
    {
        [MessagePackMember(0)]
        public List<MyBone> Bones;
    }

    /*  
    class MyScene
    {
    public:
	    vector<MaxMesh>		Items;
	    vector<MaxSkeleton>	Skeletons;

	    MSGPACK_DEFINE(Items, Skeletons);
    };
    */

    public class MyScene
    {
        [MessagePackMember(0)]
        public List<MyMesh> Items = new List<MyMesh>();
        [MessagePackMember(1)]
        public List<MySkeleton> Skeletons = new List<MySkeleton>();

    }

    /*
    class MySceneItems
    {
    public:
	    vector<string> Items;

	    MSGPACK_DEFINE(Items);
    };
    */

    public class MySceneItems
    {
        [MessagePackMember(0)]
        public List<String> Items = new List<string>();
    }
}