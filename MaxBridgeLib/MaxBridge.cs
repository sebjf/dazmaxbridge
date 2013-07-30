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

        public MaxScene myScene;
        int mesh = 0;

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

            MessagePackSerializer<MaxScene> c = MessagePackSerializer.Create<MaxScene>();
            myScene = c.Unpack(fs);

            reader.Close();
        }

        #region Scene Navigation

        public int GetNumItems()
        {
            return myScene.Items.Count;
        }

        public void SetMesh(int mesh)
        {
            this.mesh = mesh;
        }

        #endregion

        #region Skinning

        public int GetSkeletonIndex()
        {
            return myScene.Items[mesh].SkeletonIndex;
        }

        public int GetNumBones(int skeleton)
        {
            return myScene.Skeletons[skeleton].Bones.Count;
        }

        public string GetBoneName(int skeleton, int bone)
        {
            return myScene.Skeletons[skeleton].Bones[bone].Name;
        }

        public float[] GetBoneTransform(int skeleton, int bone)
        {
            float[] boneTransform = new float[7];
            boneTransform[0] = myScene.Skeletons[skeleton].Bones[bone].OriginX;
            boneTransform[1] = myScene.Skeletons[skeleton].Bones[bone].OriginY;
            boneTransform[2] = myScene.Skeletons[skeleton].Bones[bone].OriginZ;
            boneTransform[3] = myScene.Skeletons[skeleton].Bones[bone].Qx;
            boneTransform[4] = myScene.Skeletons[skeleton].Bones[bone].Qy;
            boneTransform[5] = myScene.Skeletons[skeleton].Bones[bone].Qz;
            boneTransform[6] = myScene.Skeletons[skeleton].Bones[bone].Qw;
            return boneTransform; 
        }

        #endregion

        #region Mesh & Geometry

        public int GetNumVertices()
        {
            return myScene.Items[mesh].NumVertices;
        }

        public float[] GetVertices()
        {
            return myScene.Items[mesh].Vertices.ToArray();
        }

        public int GetNumTextureVertices()
        {
            return myScene.Items[mesh].NumTextureCoordinates;
        }

        public float[] GetTextureVertices()
        {
            return myScene.Items[mesh].TextureCoordinates.ToArray();
        }

        public int GetNumFaces()
        {
            return myScene.Items[mesh].NumFaces;
        }

        public int GetNumTriangulatedFaces()
        {
            if (myScene.Items[mesh].TriangulatedFaces == null)
            {
                TriangulateFaces(myScene.Items[mesh]);
            }
            return myScene.Items[mesh].TriangulatedFaces.Length;
        }

        public int[] GetTriangulatedFaces()
        {
            if (myScene.Items[mesh].TriangulatedFaces == null)
            {
                TriangulateFaces(myScene.Items[mesh]);
            }
            return BlockCast(myScene.Items[mesh].TriangulatedFaces);
        }

        public int[] GetFaces()
        {
            return BlockCast<int>(myScene.Items[mesh].Faces);
        }

        #endregion

        #region Materials & Material Properties

        public int GetHighestMaterialSlot()
        {
            return myScene.Items[mesh].Materials.Keys.Max();
        }

        public int GetNumMaterials()
        {
            return myScene.Items[mesh].Materials.Count;
        }

        public int GetMaterialSlot(int material)
        {
            return myScene.Items[mesh].Materials[material].MaterialIndex;
        }

        public string GetMaterialName(int material)
        {
            return myScene.Items[mesh].Materials[material].MaterialName;
        }

        public string GetMaterialType(int material)
        {
            return myScene.Items[mesh].Materials[material].MaterialType;
        }

        public string GetMaterialProperty(int material, string property)
        {
            return myScene.Items[mesh].Materials[material].MaterialProperties[property];
        }

        public string TryGetMaterialProperty(int material, string property)
        {
            if (myScene.Items[mesh].Materials[material].MaterialProperties.ContainsKey(property))
                return myScene.Items[mesh].Materials[material].MaterialProperties[property];
            else
                return "";
        }

        public string[] GetMaterialProperties(int material)
        {
            return myScene.Items[mesh].Materials[material].MaterialProperties.Keys.ToArray();
        }

        public string[] GetMaterialValues(int material)
        {
            return myScene.Items[mesh].Materials[material].MaterialProperties.Values.ToArray();
        }

        #endregion

        #region Utilities

        public static T[] BlockCast<T>(byte[] source)
        {
            var array = new T[source.Length / Marshal.SizeOf(typeof(T))];
            Buffer.BlockCopy(source, 0, array, 0, source.Length);
            return array;
        }

        public static Face[] BlockCast(byte[] source)
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

        private static int[] BlockCast(Face[] faces)
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

        unsafe public static void TriangulateFaces(MaxMesh myMesh)
        {
            Face[] quadFaces = BlockCast(myMesh.Faces);

            List<Face> triangulatedFaces = new List<Face>();
            foreach (Face f in quadFaces)
            {
                Face f1;
                f1.PositionVertex1 = f.PositionVertex1;
                f1.PositionVertex2 = f.PositionVertex2;
                f1.PositionVertex3 = f.PositionVertex3;
                f1.PositionVertex4 = -1;
                f1.TextureVertex1 = f.TextureVertex1;
                f1.TextureVertex2 = f.TextureVertex2;
                f1.TextureVertex3 = f.TextureVertex3;
                f1.TextureVertex4 = -1;
                f1.MaterialId = f.MaterialId;

                triangulatedFaces.Add(f1);

                if (f.PositionVertex4 >= 0)
                {
                    Face f2;
                    f2.PositionVertex1 = f.PositionVertex1;
                    f2.PositionVertex2 = f.PositionVertex3;
                    f2.PositionVertex3 = f.PositionVertex4;
                    f2.PositionVertex4 = -1;
                    f2.TextureVertex1 = f.TextureVertex1;
                    f2.TextureVertex2 = f.TextureVertex3;
                    f2.TextureVertex3 = f.TextureVertex4;
                    f2.TextureVertex4 = -1;
                    f2.MaterialId = f.MaterialId;

                    triangulatedFaces.Add(f2);
                }
            }

            myMesh.TriangulatedFaces = triangulatedFaces.ToArray();
        }

        #endregion
    }
}
