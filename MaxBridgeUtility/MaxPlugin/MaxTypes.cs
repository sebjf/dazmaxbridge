using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxManagedBridge
{
    /* These definitions mirror the unmanaged types used within the 3ds max Mesh class - http://download.autodesk.com/global/docs/3dsmaxsdk2012/en_us/index.html */

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
}
