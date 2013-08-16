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

            SetParameter(material, "twoSided", true);
    //        SetParameter(material, "showInViewport", true);
            SetParameter(material, "adTextureLock", false);
            SetParameter(material, "adLock", false);
            SetParameter(material, "dsLock", false);

            for (int i = 0; i < myMaterial.MaterialProperties.Count; i++)
            {
                DoMaterialUpdate(material, myMaterial, myMaterial.MaterialProperties.KeysArray[i], myMaterial.MaterialProperties.ValuesArray[i]);
            }

            return material;
        }

        public void DoMaterialUpdate(IMtl maxMaterial, Material myMaterial, string property, string value)
        {
            if (value.Length <= 0)
            {
                return;
            }

            switch (property)
            {
                case "Ambient Color": SetParameter(maxMaterial, "ambient", ToIColor(value)); break;
                case "Ambient Color Map": SetParameter(maxMaterial, "ambientMap", ToMap(value)); break;
                case "Ambient Strength": SetParameter(maxMaterial, "ambientMapAmount", ToFloat(value)); break;
                case "Bump Strength Map": SetParameter(maxMaterial, "bumpMap", ToMap(value)); break;
                //"Color Map"					: (getDiffuseMap stdMaterial).map1 	= toMap myValue
                //"Diffuse Color"				: (getDiffuseMap stdMaterial).color2 	= toColour myValue
                case "Color Map"					: SetParameter(maxMaterial, "diffuseMap", ToMap(value)); break;
              //  case "Diffuse Color"					: SetParameter(maxMaterial, "diffuseMap", ToMap(value)); break;
                case "Diffuse Strength": SetParameter(maxMaterial, "diffuseMapAmount", ToFloat(value)); break;
                case "Opacity Map": SetParameter(maxMaterial, "opacityMap", ToMap(value)); break;
                case "Opacity Strength": SetParameter(maxMaterial, "opacityMapAmount", ToFloat(value)); break;
                case "Specular Color": SetParameter(maxMaterial, "specular", ToIColor(value)); break;
                case "Specular Color Map": SetParameter(maxMaterial, "specularMap", ToMap(value)); break;
                case "Specular Strength": SetParameter(maxMaterial, "specularLevel", ToFloat(value)); break;
                case "Glossiness": SetParameter(maxMaterial, "glossiness", ToFloat(value)); break;
            }

        }

        public IColor ToIColor(string str)
        {
           var components = str.Split(' ');
           return GlobalInterface.Instance.Color.Create(float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        }

        public ITexmap ToMap(string str)
        {
            IBitmapTex bitmap = GlobalInterface.Instance.NewDefaultBitmapTex;
            bitmap.SetMapName(str, false);
            bitmap.LoadMapFiles(0);
            return bitmap;
        }

        public float ToFloat(string str)
        {
            return float.Parse(str);
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
