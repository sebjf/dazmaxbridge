using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public void SetMeshMaterial(MyMesh myMesh, System.Int64 handle)
        {
            IAnimatable anim = Autodesk.Max.GlobalInterface.Instance.Animatable.GetAnimByHandle((UIntPtr)handle);

            if (anim is IMtl)
            {
                foreach (var m in GetMappedNodes(myMesh))
                {
                    m.Mtl = (anim as IMtl);
                }
            }
        }

        public IEnumerable<IINode> GetMappedNodesWithoutMaterials(MyMesh source)
        {
            return GetMappedNodes(source).Where(n => n.Mtl == null);
        }

        public bool NeedsMaterialUpdate(MyMesh source)
        {
            return GetMappedNodesWithoutMaterials(source).Any();
        }


        public IMultiMtl CreateMaterial(MyMesh myMesh)
        {
            IMultiMtl maxMaterial = GlobalInterface.Instance.NewDefaultMultiMtl;
            maxMaterial.SetNumSubMtls(myMesh.NumberOfMaterialSlots);

            foreach (var myMat in myMesh.Materials)
            {
                maxMaterial.SetSubMtlAndName(myMat.MaterialIndex, CreateStandardMaterial(myMat), ref myMat.MaterialName);
            }

            return maxMaterial;
        }

        public IMtl CreateStandardMaterial(Material myMaterial)
        {
            IStdMat2 material = GlobalInterface.Instance.NewDefaultStdMat;

            List<string> names = new List<string>();
            foreach (var p in EnumerateReferences(material))
            {
                names.Add(p.Name);
            }

            FindParameter("twoSided", material).SetValue(true);
            FindParameter("showInViewport", material).SetValue(true);
            FindParameter("adTextureLock", material).SetValue(false);
            FindParameter("adLock", material).SetValue(false);
            FindParameter("dsLock", material).SetValue(false);

           


            return material;
        }

        public void DoMtl()
        {
            
            IBitmapTex bitmapTexture = (IBitmapTex)GlobalInterface.Instance.COREInterface12.CreateInstance(SClass_ID.Texmap, GlobalInterface.Instance.Class_ID.Create(ClassIDs.BitmapTexture_A, 0));

            ITexmap rgb_mult = (ITexmap)GlobalInterface.Instance.COREInterface12.CreateInstance(SClass_ID.Texmap, GlobalInterface.Instance.Class_ID.Create(ClassIDs.RGB_Multiply_A, 0));

            IStdMat2 stdmtl = (IStdMat2)GlobalInterface.Instance.COREInterface12.CreateInstance(SClass_ID.Material, GlobalInterface.Instance.Class_ID.Create(ClassIDs.StandardMaterial_A, 0));

            IMultiMtl multimtl = (IMultiMtl)GlobalInterface.Instance.COREInterface12.CreateInstance(SClass_ID.Material, GlobalInterface.Instance.Class_ID.Create(ClassIDs.MultiMaterial, 0));

            IOSModifier m = (IOSModifier)GlobalInterface.Instance.COREInterface12.CreateInstance(SClass_ID.Osm, GlobalInterface.Instance.Class_ID.Create(ClassIDs.XFORM_A, 0));

            IList<Parameter> parameters = EnumerateReferences(stdmtl).ToList();

            IList<Parameter> parameters2 = EnumerateReferences(rgb_mult).ToList();

            IList<Parameter> parameters3 = EnumerateReferences(m).ToList();

        }




    }

}
