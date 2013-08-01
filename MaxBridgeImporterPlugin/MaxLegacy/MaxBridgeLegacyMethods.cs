using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaxManagedBridge
{
    public class MaxLegacy : MaxBridge
    {
        #region Scene Navigation

        public int GetNumItems()
        {
            return Scene.Items.Count;
        }

        protected int mesh = 0;

        public void SetMesh(int mesh)
        {
            this.mesh = mesh;
        }

        #endregion

        #region Skinning

        public int GetSkeletonIndex()
        {
            return Scene.Items[mesh].SkeletonIndex;
        }

        public int GetNumBones(int skeleton)
        {
            return Scene.Skeletons[skeleton].Bones.Count;
        }

        public string GetBoneName(int skeleton, int bone)
        {
            return Scene.Skeletons[skeleton].Bones[bone].Name;
        }

        public float[] GetBoneTransform(int skeleton, int bone)
        {
            float[] boneTransform = new float[7];
            boneTransform[0] = Scene.Skeletons[skeleton].Bones[bone].OriginX;
            boneTransform[1] = Scene.Skeletons[skeleton].Bones[bone].OriginY;
            boneTransform[2] = Scene.Skeletons[skeleton].Bones[bone].OriginZ;
            boneTransform[3] = Scene.Skeletons[skeleton].Bones[bone].Qx;
            boneTransform[4] = Scene.Skeletons[skeleton].Bones[bone].Qy;
            boneTransform[5] = Scene.Skeletons[skeleton].Bones[bone].Qz;
            boneTransform[6] = Scene.Skeletons[skeleton].Bones[bone].Qw;
            return boneTransform;
        }

        #endregion

        #region Mesh & Geometry

        public int GetNumVertices()
        {
            return Scene.Items[mesh].NumVertices;
        }

        public float[] GetVertices()
        {
            return Scene.Items[mesh].Vertices.ToArray();
        }

        public int GetNumTextureVertices()
        {
            return Scene.Items[mesh].NumTextureCoordinates;
        }

        public float[] GetTextureVertices()
        {
            return Scene.Items[mesh].TextureCoordinates.ToArray();
        }

        public int GetNumFaces()
        {
            return Scene.Items[mesh].NumFaces;
        }

        public int GetNumTriangulatedFaces()
        {
            if (Scene.Items[mesh].TriangulatedFaces == null)
            {
                TriangulateFaces(Scene.Items[mesh]);
            }
            return Scene.Items[mesh].TriangulatedFaces.Length;
        }

        public int[] GetTriangulatedFaces()
        {
            if (Scene.Items[mesh].TriangulatedFaces == null)
            {
                TriangulateFaces(Scene.Items[mesh]);
            }
            return BlockCast(Scene.Items[mesh].TriangulatedFaces);
        }

        public int[] GetFaces()
        {
            return BlockCast<int>(Scene.Items[mesh].Faces);
        }

        #endregion

        #region Materials & Material Properties

        public int GetHighestMaterialSlot()
        {
            return Scene.Items[mesh].Materials.Keys.Max();
        }

        public int GetNumMaterials()
        {
            return Scene.Items[mesh].Materials.Count;
        }

        public int GetMaterialSlot(int material)
        {
            return Scene.Items[mesh].Materials[material].MaterialIndex;
        }

        public string GetMaterialName(int material)
        {
            return Scene.Items[mesh].Materials[material].MaterialName;
        }

        public string GetMaterialType(int material)
        {
            return Scene.Items[mesh].Materials[material].MaterialType;
        }

        public string GetMaterialProperty(int material, string property)
        {
            return Scene.Items[mesh].Materials[material].MaterialProperties[property];
        }

        public string TryGetMaterialProperty(int material, string property)
        {
            if (Scene.Items[mesh].Materials[material].MaterialProperties.ContainsKey(property))
                return Scene.Items[mesh].Materials[material].MaterialProperties[property];
            else
                return "";
        }

        public string[] GetMaterialProperties(int material)
        {
            return Scene.Items[mesh].Materials[material].MaterialProperties.Keys.ToArray();
        }

        public string[] GetMaterialValues(int material)
        {
            return Scene.Items[mesh].Materials[material].MaterialProperties.Values.ToArray();
        }

        #endregion
    }
}
