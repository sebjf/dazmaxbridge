﻿using System;
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


    public enum AnimationType : int { None = 0, Keyframes = 1, PointCache = 2  };

    //class RequestParameters
    //{
    //public:
    //    vector<string>	items;
    //    AnimationType	animationType;

    //    MSGPACK_DEFINE(items, animationType);
    //};

    public class RequestParameters
    {
        [MessagePackMember(0)]
        public List<string> items = new List<string>();
        [MessagePackMember(1)]
        public int _animation;

        public AnimationType animation { get { return (AnimationType)_animation; } set { _animation = (int)value; } }
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

        public Color GetColorSafe(string key, Color fallback)
        {
            var color = GetColor(key);
            return color.HasValue ? color.Value : fallback;
        }

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

        public float GetFloatSafe(string[] keys, float fallback)
        {
            foreach (var key in keys)
            {
                if (MaterialProperties.ContainsKey(key))
                {
                    return float.Parse(MaterialProperties[key]);
                }
            }
            return fallback;
        }
    }

    /*
    class MyMeshKeyframe
    {
    public:
	    float			Time;
	    vector<float>	VertexPositions;

	    MSGPACK_DEFINE(Time, VertexPositions);
    };
    */

    public class MyMeshKeyframe
    {
        [MessagePackMember(0)]
        public float Time;
        [MessagePackMember(1)]
        public List<float> VertexPositions;
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
    unsafe public struct MyFace
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
    class MySkinningWeights
    {
    public:
	    string			BoneName;
	    vector<short>	Weights;

	    MSGPACK_DEFINE(BoneName, Weights);
    };
     */

    public class MySkinningData
    {
        [MessagePackMember(0)]
        public string BoneName;
        [MessagePackMember(1)]
        public List<ushort> Weights;
    }

    /*
    class MyMesh
    {
    public:	
	    string	Name;
	    string  ParentName;

	    int		NumVertices;
	    vector<float>	Vertices;

	    int		NumTextureVertices;
	    vector<float>	TextureVertices;

	    int		NumFaces;
	    BYTES	Faces;

	    vector<Material> Materials;

	    int		SkeletonIndex;
	    vector<MySkinningWeights> WeightMaps;

	    int		animationType;
	    vector<MyMeshKeyframe> Keyframes;
	

	    MSGPACK_DEFINE(Name, ParentName, NumVertices, Vertices, NumTextureVertices, TextureVertices, NumFaces, Faces, Materials, SkeletonIndex, WeightMaps, animationType, Keyframes);

	    vector<pair<int,QString>> _materialsToProcess;
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
        [MessagePackMember(10)]
        public List<MySkinningData> WeightMaps;

        [MessagePackMember(11)]
        public int _animationType;
        [MessagePackMember(12)]
        public List<MyMeshKeyframe> Keyframes;

        public AnimationType AnimationType { get { return (AnimationType)_animationType; } }

        /* The following properties are on the receiver side only (not part of the message from Daz) */

        public MyFace[] TriangulatedFaces = null;

        public string CharacterName {
            get
            {
                if (string.IsNullOrWhiteSpace(ParentName))
                {
                    return Name;
                }
                return ParentName;
            }
        }

        public MySkeleton Skeleton;

    }

    /*
    class MyBone
    {
    public:
	    string	Name;
	    string	ParentName;

	    float	OriginX;
	    float	OriginY;
	    float	OriginZ;

	    float	EndpointX;
	    float	EndpointY;
	    float	EndpointZ;

	    double	Qx;
	    double	Qy;
	    double	Qz;
	    double	Qw;

	    MSGPACK_DEFINE(Name,ParentName,OriginX,OriginY,OriginZ,EndpointX,EndpointY,EndpointZ,Qx,Qy,Qz,Qw);
    };
     */

    public class MyBone
    {
        [MessagePackMember(0)]
        public string Name;
        [MessagePackMember(1)]
        public string ParentName;

        [MessagePackMember(2)]
        public float OriginX;
        [MessagePackMember(3)]
        public float OriginY;
        [MessagePackMember(4)]
        public float OriginZ;

        [MessagePackMember(5)]
        public float EndpointX;
        [MessagePackMember(6)]
        public float EndpointY;
        [MessagePackMember(7)]
        public float EndpointZ;

        [MessagePackMember(8)]
        public double Qx;
        [MessagePackMember(9)]
        public double Qy;
        [MessagePackMember(10)]
        public double Qz;
        [MessagePackMember(11)]
        public double Qw;
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
    class MySceneInformation
    {
    public:
	    vector<string>					TopLevelItemNames;

	    MSGPACK_DEFINE(TopLevelItemNames);

    };
    */

    public class MySceneInformation
    {
        [MessagePackMember(0)]
        public List<String> TopLevelItemNames = new List<string>();
    }
}