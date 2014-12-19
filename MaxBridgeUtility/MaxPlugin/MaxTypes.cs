using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    /* http://code.google.com/p/snakengine/source/browse/trunk/OtherLibs/3dmax+sdk/2012/include/INodeTransformModes.h?r=5 */
    enum PivotMode : int
    {
        PIV_NONE = 0,
        PIV_PIVOT_ONLY = 1,
        PIV_OBJECT_ONLY = 2,
        PIV_HIERARCHY_ONLY = 3
    }

    /*http://docs.autodesk.com/3DSMAX/15/ENU/3ds-Max-SDK-Programmer-Guide/index.html?url=files/GUID-B41DC781-221E-4DE3-8AA1-EC3C2666FC5C.htm,topicNumber=d30e22562 */
    public static class ClassIDs
    {
        public const uint mr_SSS2_Skin_A = 2004030991;
        public const uint mr_SSS2_Skin_B = 2251076473;
        public const uint XFORM_A = 622942244;
        public const uint BitmapTexture_A = 576;
        public const uint RGB_Multiply_A = 656;
        public const uint StandardMaterial_A = 2;
        public const uint MultiMaterial = 512;
        public const uint Skin_A = 9815843;
        public const uint Skin_B = 87654;
        public const uint BONE_OBJ_A = 683634317;
        public const uint BONE_OBJ_B = 785164352;

    }

    public static class InterfaceIDs
    {
        public const uint I_SKINIMPORTDATA = 0x00020000;
    }

    public static class MaxFlags
    {
        //maxapi.h
        public const int VP_DONT_SIMPLIFY = 0x0002;
    }

    public static class MaxExtensions
    {
        public static bool EqualsClassID(this IClass_ID classA, uint a, uint b)
        {
            return ((classA.PartA == a) && (classA.PartB == b));
        }

        public static bool EqualsClassID(this IClass_ID classA, IClass_ID classB)
        {
            return ((classA.PartA == classB.PartA) && (classA.PartB == classB.PartB));
        }
    }

    /* These definitions mirror the unmanaged types used within the 3ds max Mesh class - http://download.autodesk.com/global/docs/3dsmaxsdk2012/en_us/index.html */

    #pragma warning disable 0649    //0649 is the 'member is never used' warning, which is of little help as these definitions must match existing unmanaged types.

    /* The Point3 type is used to store vertices and normals, the managed equivalent is IPoint3 
     * http://download.autodesk.com/global/docs/3dsmaxsdk2012/en_us/index.html */

    unsafe struct Point3
    {
        public float x;
        public float y;
        public float z;
    }

    /* The TVFace type is used to store offsets into the texture vertex array so that the texture and geometric surfaces can be defined independently, the managed equivalent is ITVFace
     * http://download.autodesk.com/global/docs/3dsmaxsdk2012/en_us/index.html */

    unsafe struct TVFace
    {
        public UInt32 t1;   //the correct defintion is a DWORD[3] array but we split it out to make creating the face easier
        public UInt32 t2;
        public UInt32 t3; 
    }

    /* so we can do the copy with one assignment */
    unsafe struct Indices3
    {
        public UInt32 v1;
        public UInt32 v2;
        public UInt32 v3;
    }

    /* The Face type stores the offets into the vertex array to create the geometric surface. It also stores smoothing group information, edge visibility flags and material information.
     * http://download.autodesk.com/global/docs/3dsmaxsdk2012/en_us/index.html */

    unsafe struct Face
    {
        public Indices3 v;
        public UInt32 smGroup;
        public UInt32 flags;

    }

    #pragma warning restore 0649
}
