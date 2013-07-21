using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using MsgPack;
using MsgPack.Serialization;


namespace MaxBridgeLib
{
    public class MaxBridge
    {
        MaxMesh myMesh;

        public const int FLOATS_PER_VERTEX = 3;
        public const int INTS_PER_FACE = 4;

        public string Test()
        {
            return "Max Bridge Managed Library v.0 (in development)";
        }

        public void LoadFromFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);

            MessagePackSerializer<MaxMesh> c = MessagePackSerializer.Create<MaxMesh>();
            myMesh = c.Unpack(fs);
        }

        public int GetNumVertices()
        {
            return myMesh.NumVertices;
        }

        public float[] GetVertices()
        {
            return BlockCast<float>(myMesh.Vertices);
        }

        public int GetNumFaces()
        {
            return myMesh.NumFaces;
        }

        public int[] GetFaces()
        {
            return BlockCast<int>(myMesh.Faces);
        }

        public int[] GetFaceMaterialIDs()
        {
            return BlockCast<int>(myMesh.FaceMaterialIDs);
        }

        private T[] BlockCast<T>(byte[] source)
        {
            var array = new T[source.Length / Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(source, 0, array, 0, source.Length);
            return array;
        }
    }
}
