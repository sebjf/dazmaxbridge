using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack;
using MsgPack.Serialization;

namespace MaxBridgeLib
{
    public class MaxMesh
    {
        [MessagePackMember(0)]
        public int NumVertices;
        [MessagePackMember(1)]
        public int VerticesLengthInBytes;
        [MessagePackMember(2)]
        public byte[] Vertices;

        [MessagePackMember(3)]
        public int NumFaces;
        [MessagePackMember(4)]
        public int FacesLengthInBytes;
        [MessagePackMember(5)]
        public byte[] Faces;
        [MessagePackMember(6)]
        public byte[] FaceMaterialIDs;
    }
}
