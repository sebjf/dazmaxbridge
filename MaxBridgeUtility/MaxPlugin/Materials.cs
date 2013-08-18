using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Text.RegularExpressions;

namespace MaxManagedBridge
{
    public partial class MaxPlugin : MaxBridge
    {
        public float BumpScalar = 500;

        public IMultiMtl CreateMaterial(MyMesh myMesh)
        {
            IMultiMtl maxMaterial = GlobalInterface.Instance.NewDefaultMultiMtl;
            maxMaterial.SetNumSubMtls(myMesh.NumberOfMaterialSlots);

            foreach (var myMat in myMesh.Materials)
            {
                maxMaterial.SetSubMtlAndName(myMat.MaterialIndex, CreateStandardMaterialMaxScript(myMat), ref myMat.MaterialName);
            }

            return maxMaterial;
        }

        #region Creating a material natively

        public IMtl CreateStandardMaterial(Material myMaterial)
        {
            throw new NotImplementedException("This method is not complete. If you know what you are doing then you can comment this out and continue to debug. Good luck! If not, use CreateStandardMaterialMaxScript instead.");

            IStdMat2 material = GlobalInterface.Instance.NewDefaultStdMat;

            SetParameter(material, "twoSided", true);
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
              //case "Diffuse Color"					: SetParameter(maxMaterial, "diffuseMap", ToMap(value)); break;
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

        #endregion

        /// <summary>
        /// Creates a StandardMaterial instance for the provided material by generating and executing a MaxScript. 
        /// This is different from calling back to a MaterialProcessor. This call should be 'transparent' (i.e. you
        /// can use it in place of CreateStandardMaterial() or any other similar call with no noticeable difference
        /// in functionality)
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public IMtl CreateStandardMaterialMaxScript(Material material)
        {
            MaxScriptStandardMaterial maxMaterial = new MaxScriptStandardMaterial();

            maxMaterial.Ambient = material.GetColor("Ambient Color");
            maxMaterial.AmbientMap = material.GetString("Ambient Color Map");
            maxMaterial.AmbientMapAmount = material.GetFloat("Ambient Strength") * 100;
            maxMaterial.BumpMap = material.GetString("Bump Strength Map");
            maxMaterial.DiffuseMap = material.GetString("Color Map");
            maxMaterial.Diffuse = material.GetColor("Diffuse Color");
            maxMaterial.DiffuseMapAmount = material.GetFloat("Diffuse Strength") * 100;
            maxMaterial.OpacityMap = material.GetString("Opacity Map");
            maxMaterial.OpacityMapAmount = material.GetFloat("Opacity Strength") * 100;
            maxMaterial.Specular = material.GetColor("Specular Color");
            maxMaterial.SpecularMap = material.GetString("Specular Color Map");
            maxMaterial.SpecularLevel = material.GetFloat("Specular Strength") * 100;
            maxMaterial.Glossiness = material.GetFloat("Glossiness") * 100;
            maxMaterial.U_tiling = material.GetFloat("Horizontal Tiles");
            maxMaterial.V_tiling = material.GetFloat("Vertical Tiles");

            maxMaterial.BumpMapAmount = (material.GetFloatSafe("Positive Bump", 0) - material.GetFloatSafe("Negative Bump", 0)) * material.GetFloatSafe("Bump Strength", 0.03f) * BumpScalar;

            string script = maxMaterial.MakeScript();
         
            string handle_string = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(script);
            handle_string = Regex.Replace(handle_string, "\\D", string.Empty);
            System.Int64 handle = System.Int64.Parse(handle_string);
            if (handle < 0)
            {
                throw new Exception(string.Format("MaxScript could not return a valid handle. There is an error in the script: \n\n{0}\n\n",script));
            }

            return (Convert(handle) as IMtl);

        }




    }

}
