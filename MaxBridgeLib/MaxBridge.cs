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
        #region Local Members

        MaxMesh myMesh;

        public const int FLOATS_PER_VERTEX = 3;
        public const int INTS_PER_FACE = 4;

        #endregion

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

        #region Mesh & Geometry

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

        public int GetNumTriangulatedFaces()
        {
            if (myMesh.TriangulatedFaces == null)
            {
                TriangulateFaces(myMesh);
            }
            return myMesh.TriangulatedFaces.Length;
        }

        public int[] GetTriangulatedFaces()
        {
            if (myMesh.TriangulatedFaces == null)
            {
                TriangulateFaces(myMesh);
            }
            return BlockCast(myMesh.TriangulatedFaces);
        }

        public int[] GetFaces()
        {
            return BlockCast<int>(myMesh.Faces);
        }

        #endregion

        #region Materials & Material Properties

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

        #endregion

        #region Utilities

        private T[] BlockCast<T>(byte[] source)
        {
            var array = new T[source.Length / Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(source, 0, array, 0, source.Length);
            return array;
        }

        private Face[] BlockCast(byte[] source)
        {
            int elements = source.Length / Marshal.SizeOf(typeof(Face));
            Face[] dst = new Face[elements];
            unsafe
            {
                fixed (byte* pSrc = source)
                {
                    for (int i = 0; i < dst.Length; i++)
                    {
                        dst[i] = ((Face*)pSrc)[i];
                    }
                }
            }

            return dst;
        }

        private int[] BlockCast(Face[] faces)
        {
            int dstElements = (Marshal.SizeOf(typeof(Face)) / Marshal.SizeOf(typeof(int))) * faces.Length;
            int[] dst = new int[dstElements];

            unsafe{
                fixed( int* pDst = dst)
                {
                    for(int i = 0; i < faces.Length; i++)
                    {
                        ((Face*)pDst)[i] = faces[i];
                    }
                }
            }

            return dst;
        }

        #endregion

        #region Mesh Processing

        unsafe private void TriangulateFaces(MaxMesh myMesh)
        {
            Face[] quadFaces = BlockCast(myMesh.Faces);

            List<Face> triangulatedFaces = new List<Face>();
            foreach (Face f in quadFaces)
            {
                Face localFace = f;

                int fv1 = localFace.PositionVertices[0];
                int fv2 = localFace.PositionVertices[1];
                int fv3 = localFace.PositionVertices[2];
                int fv4 = localFace.PositionVertices[3];

                int tv1 = localFace.TextureVertices[0];
                int tv2 = localFace.TextureVertices[1];
                int tv3 = localFace.TextureVertices[2];
                int tv4 = localFace.TextureVertices[3];

                int fmaterial = localFace.MaterialId;

                Face f1;

                f1.PositionVertices[0] = fv1;
                f1.PositionVertices[1] = fv2;
                f1.PositionVertices[2] = fv3;
                f1.TextureVertices[0] = tv1;
                f1.TextureVertices[1] = tv2;
                f1.TextureVertices[2] = tv3;
                f1.MaterialId = fmaterial;

                triangulatedFaces.Add(f1);

                if (fv1 >= 0)
                {
                    Face f2;

                    f2.PositionVertices[0] = fv1;
                    f2.PositionVertices[1] = fv3;
                    f2.PositionVertices[2] = fv4;
                    f2.TextureVertices[0] = tv1;
                    f2.TextureVertices[1] = tv3;
                    f2.TextureVertices[2] = tv4;
                    f2.MaterialId = fmaterial;

                    triangulatedFaces.Add(f2);
                }
            }


            myMesh.TriangulatedFaces = triangulatedFaces.ToArray();
        }

        #endregion
    }
}
