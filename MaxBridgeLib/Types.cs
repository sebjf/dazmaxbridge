using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack;
using MsgPack.Serialization;

namespace MaxBridgeLib
{
    public class Material
    {
        [MessagePackMember(0)]
	    public string MaterialName;
        [MessagePackMember(1)]
	    public int MaterialIndex; //index as known by the mesh (i.e. the material slot)
    }

    public class MaxMesh
    {
        [MessagePackMember(0)]
        public int NumVertices;
        [MessagePackMember(1)]
        public byte[] Vertices;

        [MessagePackMember(2)]
        public int NumFaces;
        [MessagePackMember(3)]
        public byte[] Faces;
        [MessagePackMember(4)]
        public byte[] FaceMaterialIDs;

        [MessagePackMember(5)]
        public List<Material> Materials;
    }
}
