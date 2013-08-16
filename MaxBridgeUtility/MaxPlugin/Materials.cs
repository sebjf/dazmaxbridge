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
            //CreateStandardMaterialMaxScript - because the only thing worse than code generation is using Max's SDK
            MaxScriptObjectBuilder mxsMaterial = new MaxScriptObjectBuilder("stdMaterial");

            //The Commands property is just a list, we can add any arbitrary thing to it
            mxsMaterial.Commands.Add("fn getDiffuseMap stdMaterial =( if(stdMaterial.diffuseMap == undefined) then (stdMaterial.diffuseMap = RGB_Multiply()) return stdMaterial.diffuseMap)");

            mxsMaterial.Commands.Add(string.Format("stdMaterial = StandardMaterial name:(\"{0}\")", material.MaterialName));

            mxsMaterial["twoSided"]         = "true";
            mxsMaterial["showInViewport"]   = "true";
            mxsMaterial["adTextureLock"]    = "false";
            mxsMaterial["adLock"]           = "false";
            mxsMaterial["dsLock"]           = "false";

            mxsMaterial["ambient"]      = ToColourCommand(material["Ambient Color"]);
            mxsMaterial["ambientMap"]   = ToMapCommand(material["Ambient Color Map"]);
            mxsMaterial["ambientMapAmount"] = ToPercentCommand(material["Ambient Strength"]);
            mxsMaterial["bumpMap"]      = ToMapCommand(material["Bump Strength Map"]);

            if (material.MaterialProperties.ContainsKey("Color Map"))
            {
                mxsMaterial.Commands.Add("(getDiffuseMap stdMaterial).map1 = " + ToMapCommand(material["Color Map"]));
            }

            if (material.MaterialProperties.ContainsKey("Diffuse Color"))
            {
                mxsMaterial.Commands.Add("(getDiffuseMap stdMaterial).color2 = " + ToColourCommand(material["Diffuse Color"]));
            }

            mxsMaterial["diffuseMapAmount"]     = ToPercentCommand(material["Diffuse Strength"]);


            if (material["Opacity Map"].Length > 0)
            {
                mxsMaterial["opacityMap"]       = ToMapCommand(material["Opacity Map"]);
                mxsMaterial["opacityMapAmount"] = ToPercentCommand(material["Opacity Strength"]);
            }
            else
            {
                mxsMaterial["opacity"]  = ToPercentCommand(material["Opacity Strength"]);
            }

            mxsMaterial["specular"]     = ToColourCommand(material["Specular Color"]);
            mxsMaterial["specularMap"]  = ToMapCommand(material["Specular Color Map"]);
            mxsMaterial["specularLevel"] = ToPercentCommand(material["Specular Strength"]);
            mxsMaterial["glossiness"]   = ToPercentCommand(material["Glossiness"]);

            float bumpAmount = (float.Parse(material.MaterialProperties.SafeGet("Positive Bump", "0")) - float.Parse(material.MaterialProperties.SafeGet("Negative Bump", "0"))) * float.Parse(material.MaterialProperties.SafeGet("Bump Strength", "0.03")) * BumpScalar;
            mxsMaterial.Commands.Add(string.Format("stdMaterial.bumpMapAmount = {0}", (bumpAmount * 100)));

            string u_tiling = material.MaterialProperties.SafeGet( "Horizontal Tiles", "1");
            string v_tiling = material.MaterialProperties.SafeGet( "Vertical Tiles", "1");

            mxsMaterial.Commands.Add("maps = (GetClassInstances bitmapTexture target:stdMaterial)");
            mxsMaterial.Commands.Add(string.Format("for map in maps do( map.coords.u_tiling = {0}; map.coords.v_tiling = {1}; )", u_tiling, v_tiling));

            List<string> log = new List<string>();
            foreach (var s in mxsMaterial.Commands)
            {
                var r = ManagedServices.MaxscriptSDK.ExecuteBooleanMaxscriptQuery(s);
                log.Add(string.Format("{0} -> {1}", s, r));
            }

            string handle_string = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery("(getHandleByAnim stdMaterial) as String");
            handle_string = Regex.Replace(handle_string, "\\D", string.Empty);
            System.Int64 handle = System.Int64.Parse(handle_string);
            if (handle < 0)
            {
                throw new Exception("MaxScript could not return a valid handle. There is an error in the script");
            }

            return (Convert(handle) as IMtl);

        }




    }

}
