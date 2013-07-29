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

        MaxScene myScene;
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
                f1.PositionVertices[3] = -1;
                f1.TextureVertices[0] = tv1;
                f1.TextureVertices[1] = tv2;
                f1.TextureVertices[2] = tv3;
                f1.TextureVertices[3] = -1;
                f1.MaterialId = fmaterial;

                triangulatedFaces.Add(f1);

                if (fv4 >= 0)
                {
                    Face f2;

                    f2.PositionVertices[0] = fv1;
                    f2.PositionVertices[1] = fv3;
                    f2.PositionVertices[2] = fv4;
                    f2.PositionVertices[3] = -1;
                    f2.TextureVertices[0] = tv1;
                    f2.TextureVertices[1] = tv3;
                    f2.TextureVertices[2] = tv4;
                    f2.TextureVertices[3] = -1;
                    f2.MaterialId = fmaterial;

                    triangulatedFaces.Add(f2);
                }
            }


            myMesh.TriangulatedFaces = triangulatedFaces.ToArray();
        }

        #endregion
    }
}
