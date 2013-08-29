using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using Autodesk.Max;

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

        #region Colour Conversion

        Color? ConvertColour(Color? colour)
        {
            if (colour == null)
                return null;

            return Color.FromArgb(colour.Value.A, MaxColourValues[colour.Value.R], MaxColourValues[colour.Value.G], MaxColourValues[colour.Value.B]);
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

            maxMaterial.name = material.MaterialName;
            maxMaterial.Ambient = ConvertColour(material.GetColor("Ambient Color"));
            maxMaterial.AmbientMap = material.GetString("Ambient Color Map");
            maxMaterial.AmbientMapAmount = material.GetFloat("Ambient Strength") * 100;
            maxMaterial.BumpMap = material.GetString("Bump Strength Map");
            maxMaterial.DiffuseMap = material.GetString("Color Map");
            maxMaterial.Diffuse = ConvertColour(material.GetColor("Diffuse Color"));
            maxMaterial.DiffuseMapAmount = material.GetFloat("Diffuse Strength") * 100;
            maxMaterial.OpacityMap = material.GetString("Opacity Map");
            maxMaterial.OpacityMapAmount = material.GetFloat("Opacity Strength") * 100;
            maxMaterial.Specular = ConvertColour(material.GetColor("Specular Color"));
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

        protected int[] MaxColourValues = { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 9, 10, 10, 11, 11, 11, 12, 12, 13, 13, 14, 14, 15, 15, 16, 16, 17, 17, 18, 18, 19, 19, 20, 20, 21, 21, 22, 23, 23, 24, 24, 25, 26, 26, 27, 28, 28, 29, 30, 30, 31, 32, 32, 33, 34, 35, 35, 36, 37, 38, 38, 39, 40, 41, 42, 42, 43, 44, 45, 46, 47, 48, 49, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 73, 74, 75, 76, 77, 78, 79, 81, 82, 83, 84, 85, 87, 88, 89, 90, 92, 93, 94, 95, 97, 98, 99, 101, 102, 103, 105, 106, 107, 109, 110, 112, 113, 114, 116, 117, 118, 120, 122, 123, 125, 126, 128, 129, 131, 132, 134, 135, 137, 138, 140, 142, 143, 144, 146, 148, 150, 151, 153, 155, 156, 158, 160, 162, 163, 165, 167, 168, 170, 172, 174, 176, 177, 179, 181, 183, 185, 187, 188, 190, 192, 193, 196, 198, 200, 202, 204, 206, 208, 210, 212, 214, 216, 218, 220, 222, 224, 226, 228, 230, 232, 234, 236, 238, 240, 243, 245, 247, 249, 251, 253, 255 };


    }

}
