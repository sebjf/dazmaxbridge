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
            return myMesh.Vertices.ToArray();
        }

        public int GetNumTextureVertices()
        {
            return myMesh.NumTextureCoordinates;
        }

        public float[] GetTextureVertices()
        {
            return myMesh.TextureCoordinates.ToArray();
        }

        public int GetNumFaces()
        {
            return myMesh.NumFaces;
        }

        public int[] GetFaces()
        {
            return BlockCast<int>(myMesh.Faces);
        }

        public int GetHighestMaterialSlot()
        {
            int max = 0;
            foreach (Material m in myMesh.Materials){
                if (m.MaterialIndex > max)
                    max = m.MaterialIndex;
            }
            return max;
        }

        public int GetNumMaterials()
        {
            return myMesh.Materials.Count;
        }

        public int GetMaterialSlot(int material)
        {
            return myMesh.Materials[material].MaterialIndex;
        }

        public string GetMaterialName(int material)
        {
            return myMesh.Materials[material].MaterialName;
        }

        public string GetMaterialType(int material)
        {
            return myMesh.Materials[material].MaterialType;
        }

        public string GetMaterialProperty(int material, string property)
        {
            return myMesh.Materials[material].MaterialProperties[property];
        }

        public string TryGetMaterialProperty(int material, string property)
        {
            if (myMesh.Materials[material].MaterialProperties.ContainsKey(property))
                return myMesh.Materials[material].MaterialProperties[property];
            else
                return "";
        }

        public string[] GetMaterialProperties(int material)
        {
            return myMesh.Materials[material].MaterialProperties.Keys.ToArray();
        }

        public string[] GetMaterialValues(int material)
        {
            return myMesh.Materials[material].MaterialProperties.Values.ToArray();
        }

        private T[] BlockCast<T>(byte[] source)
        {
            var array = new T[source.Length / Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(source, 0, array, 0, source.Length);
            return array;
        }
    }
}
