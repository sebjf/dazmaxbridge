using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using Autodesk.Max;
using System.Windows.Forms;

namespace MaxManagedBridge
{
    public interface IMaterialCreationOptions
    {
        IMaxScriptMaterialProperties GetNewMaterial();
        string MaterialName { get; }
        object BindingInfo { get; set; } //This is for use by the GUI, don't touch it
    }

    public partial class MaxPlugin : MaxBridge
    {
        protected IMaterialCreationOptions materialOptions = new MaterialOptionsMentalRayArchAndDesign();
        public IMaterialCreationOptions MaterialOptions
        {
            set
            {
                if (value is IMaterialCreationOptions)
                {
                    materialOptions = value;
                }
                else
                {
                    string message = "MaterialOptions must be valid object implementing GetNewMaterial()";
                    Log.Add(message);
                    throw new ArgumentException(message);
                }
            }
        }

        public IMaterialCreationOptions[] AvailableMaterials = { new MaterialOptionsMentalRayArchAndDesign(), new MaterialOptionsVRayMaterial(), new MaterialOptionsStandardMaterial() };

        public IMultiMtl CreateMaterial(MyMesh myMesh)
        {
            IMultiMtl maxMaterial = globalInterface.NewDefaultMultiMtl;
            maxMaterial.SetNumSubMtls(myMesh.NumberOfMaterialSlots);

            foreach (var myMat in myMesh.Materials)
            {
                maxMaterial.SetSubMtlAndName(myMat.MaterialIndex, CreateMaterialMaxScript(myMat), ref myMat.MaterialName);
            }

            return maxMaterial;
        }

        public string PrintMaterialProperties(MyScene scene)
        {
            List<String> lines = new List<string>();
            foreach (MyMesh m in scene.Items)
            {
                lines.Add("-------------------------------------------");
                lines.Add("Mesh Name: " + m.Name);
                lines.Add("-------------------------------------------");

                foreach (var mat in m.Materials)
                {
                    lines.Add("        -------------------        ");
                    lines.Add("Material Name: " + mat.MaterialName);
                    lines.Add("Material Index: " + mat.MaterialIndex);
                    lines.Add("Material Type: " + mat.MaterialType);
                    lines.Add("        -------------------        ");

                    foreach (KeyValuePair<string, string> kvp in mat.MaterialProperties)
                    {
                        lines.Add(kvp.Key + ": " + kvp.Value);
                    }

                    lines.Add("        -------------------        ");
                }

                lines.Add("END");
            }

            string final = "";
            foreach (var s in lines)
            {
                final += (s + Environment.NewLine);
            }

            return final;
        }

        /// <summary>
        /// Creates a StandardMaterial instance for the provided material by generating and executing MaxScript. 
        /// This is different from calling back to a MaterialProcessor. This call should be 'transparent' (i.e. you
        /// can use it in place of CreateStandardMaterial() or any other similar call with no noticeable difference
        /// in functionality)
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public IMtl CreateMaterialMaxScript(Material material)
        {
            IMaxScriptMaterialProperties maxMaterial = materialOptions.GetNewMaterial();

            /* All percentages should be between 0-1 and any conversions done in the script generation */

            maxMaterial.Name = material.MaterialName;
            maxMaterial.Ambient = Defaults.AmbientGammaCorrection ? ConvertColour(material.GetColor("Ambient Color")) : material.GetColor("Ambient Color");
            maxMaterial.AmbientMap = material.GetString("Ambient Color Map");
            maxMaterial.AmbientMapAmount = material.GetFloat("Ambient Strength");
            maxMaterial.BumpMap = material.GetString("Bump Strength Map");
            maxMaterial.DiffuseMap = material.GetString("Color Map");
            maxMaterial.Diffuse = ConvertColour(material.GetColor("Diffuse Color"));
            maxMaterial.DiffuseMapAmount = material.GetFloat("Diffuse Strength");
            maxMaterial.OpacityMap = material.GetString("Opacity Map");
            maxMaterial.OpacityMapAmount = material.GetFloat("Opacity Strength");
            maxMaterial.Specular = ConvertColour(material.GetColor("Specular Color"));
            maxMaterial.SpecularMap = material.GetString("Specular Color Map");
            maxMaterial.SpecularLevel = material.GetFloat("Specular Strength");
            maxMaterial.Glossiness = material.GetFloat("Glossiness");
            maxMaterial.U_tiling = material.GetFloat("Horizontal Tiles");
            maxMaterial.V_tiling = material.GetFloat("Vertical Tiles");

            maxMaterial.BumpMapAmount = ((material.GetFloatSafe(new string[]{"Positive Bump", "Bump Maximum"}, 0.1f) - material.GetFloatSafe(new string[]{"Negative Bump", "Bump Minimum"}, -0.1f)) * material.GetFloatSafe("Bump Strength", 1.0f));

            string script = maxMaterial.MakeScript();

            try
            {
                string handle_string = ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(script);
                handle_string = Regex.Replace(handle_string, "\\D", string.Empty);
                System.Int64 handle = System.Int64.Parse(handle_string);
                IMtl nativeMtl = (Convert(handle) as IMtl);
                if (nativeMtl == null)
                {
                    throw new Exception("Could not resolve string from MaxScript to handle of material object");
                }
                return nativeMtl;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show(
                    "Copy the full script out of this window with Ctrl+C and run it in 3DS Max listener without the parenthesis to find the error.\n\n" + script,
                    "Exception: Could not create material.",
                    MessageBoxButtons.OK);
            }

            return null;
        }

        #region Gamma Correction

        public static Color ConvertColour(Color colour)
        {
            return Color.FromArgb(colour.A, CorrectGamma(colour.R), CorrectGamma(colour.G), CorrectGamma(colour.B));
        }

        public static Color? ConvertColour(Color? colour)
        {
            if (colour == null)
                return null;

            return ConvertColour(colour.Value);
        }

        /*This is far from ideal but since Daz doesn't follow a linear workflow we can assume the colour channels will never go above white*/

        protected static byte CorrectGamma(byte v)
        {
            float c = (float)v;
            return (byte)(Math.Pow(c / 255.0f, 2.2f) * 255.0f);
        }

        #endregion
    }

    public interface IMaxScriptMaterialProperties
    {
        string Name { set; }
        Nullable<Color> Ambient { set; }
        string AmbientMap { set; }
        Nullable<float> AmbientMapAmount { set; }
        string BumpMap { set; }
        Nullable<float> BumpMapAmount { set; }
        Nullable<Color> Diffuse { set; }
        string DiffuseMap { set; }
        Nullable<float> DiffuseMapAmount { set; }
        string OpacityMap { set; }
        Nullable<float> OpacityMapAmount { set; }
        Nullable<Color> Specular { set; }
        string SpecularMap { set; }
        Nullable<float> SpecularLevel { set; }
        Nullable<float> Glossiness { set; }
        Nullable<float> U_tiling { set; }
        Nullable<float> V_tiling { set; }

        string MakeScript();
    }

    public abstract class MaxScriptMaterial : IMaxScriptMaterialProperties
    {
        public string Name { get; set; }

        public Nullable<Color> Ambient { set { if (value != null) ambient = value.Value; } }
        public string AmbientMap { set { if (value != null) ambientMap = value; } }
        public Nullable<float> AmbientMapAmount { set { if (value != null) ambientMapAmount = value.Value; } }
        public string BumpMap { set { if (value != null) bumpMap = value; } }
        public Nullable<float> BumpMapAmount { set { if (value != null) bumpMapAmount = value.Value; } }
        public Nullable<Color> Diffuse { set { if (value != null) diffuse = value.Value; } }
        public string DiffuseMap { set { if (value != null) diffuseMap = value; } }
        public Nullable<float> DiffuseMapAmount { set { if (value != null) diffuseMapAmount = value.Value; } }
        public string OpacityMap { set { if (value != null) opacityMap = value; } }
        public Nullable<float> OpacityMapAmount { set { if (value != null) opacityMapAmount = value.Value; } }
        public Nullable<Color> Specular { set { if (value != null) specular = value.Value; } }
        public string SpecularMap { set { if (value != null) specularMap = value; } }
        public Nullable<float> SpecularLevel { set { if (value != null) specularLevel = value.Value; } }
        public Nullable<float> Glossiness { set { if (value != null) glossiness = value.Value; } }
        public Nullable<float> U_tiling { set { if (value != null) u_tiling = value.Value; } }
        public Nullable<float> V_tiling { set { if (value != null) v_tiling = value.Value; } }

        public bool showInViewport = true;

        protected Color ambient = Color.Black;
        protected string ambientMap = null;
        protected float ambientMapAmount = 1.0f;

        protected string bumpMap = null;
        protected float bumpMapAmount = 1.0f;

        protected Color diffuse = Color.FromArgb(127, 127, 127);
        protected string diffuseMap = null;
        protected float diffuseMapAmount = 1.0f;

        protected string opacityMap = null;
        protected float opacityMapAmount = 1.0f;

        protected Color specular = Color.FromArgb(230, 230, 230);
        protected string specularMap = null;
        protected float specularLevel = 0.0f;
        protected float glossiness = 10.0f;

        protected float u_tiling = 1;
        protected float v_tiling = 1;

        public abstract string MakeScript();
    }

    public class MaterialOptionsMentalRayArchAndDesign : IMaterialCreationOptions
    {
        /* Here are the options that should be exposed through the UI - they will be databound to controls on the form when this option is selected in the drop down list */

        public bool  MapFilteringDisable { get; set; }
        public bool  AOEnable { get; set; }
        public int   AODistance { get; set; }
        public float GlossScalar { get; set; }
        public float BumpScalar { get; set; }

        public MaterialOptionsMentalRayArchAndDesign()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            AOEnable = Defaults.MentalRay_AOEnable;
            AODistance = Defaults.MentalRay_AODistance;
            GlossScalar = Defaults.MentalRay_GlossScalar;
            BumpScalar = Defaults.MentalRay_BumpScalar;
        }
    
        public IMaxScriptMaterialProperties GetNewMaterial()
        {
 	        MaxScriptMentalRayArchAndDesignMaterial material = new MaxScriptMentalRayArchAndDesignMaterial();
            material.DisableFiltering = MapFilteringDisable;
            material.AOEnable = AOEnable;
            material.AORayDistance = AODistance;
            material.GlossScalar = GlossScalar;
            material.BumpScalar = BumpScalar;
            return material;
        }

        public string MaterialName
        {
            get { return "MentalRay Arch & Design Material"; }
        }

        public object BindingInfo { get; set; }
    }

    public class MaxScriptMentalRayArchAndDesignMaterial : MaxScriptMaterial
    {
        public MaxScriptMentalRayArchAndDesignMaterial()
        {
            Name = "mrArchDesignMaxScriptMaterial";
        }

        public bool AOEnable = Defaults.MentalRay_AOEnable;
        public int  AORayDistance = Defaults.MentalRay_AODistance;
        public bool DisableFiltering = Defaults.MapFilteringDisable;
        public float GlossScalar = Defaults.MentalRay_GlossScalar;
        public float BumpScalar = Defaults.MentalRay_BumpScalar;
        public bool EnableHighlightsFGOnly = Defaults.MentalRay_EnableHighlightsFGOnly;

        public override string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = Arch___Design__mi name:(\"{0}\")", Name));

            Commands.Add(string.Format("material.showInViewport = {0}", showInViewport));

            Commands.Add(string.Format("material.diffuse_weight = {0}", diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.Diffuse_Color_Map = CompositeTextureMap()"));
            Commands.Add(string.Format("material.Diffuse_Color_Map.add()"));
            Commands.Add(string.Format("material.Diffuse_Color_Map.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", diffuseMap);
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, diffuse.R, diffuse.G, diffuse.B));

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.coords.u_tiling = {0}; material.Diffuse_Color_Map.mapList[1].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", ambientMap);
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, ambient.R * ambientMapAmount, ambient.G * ambientMapAmount, ambient.B * ambientMapAmount));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.coords.u_tiling = {0}; material.Diffuse_Color_Map.mapList[2].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mask[2] = material.Diffuse_Color_Map.mapList[1]"));

            if (bumpMap != null)
            {
                Commands.Add(string.Format("material.bump_map = (bitmapTexture filename:\"{0}\")", bumpMap));
                Commands.Add(string.Format("material.Bump_Map_Amount = {0}", (bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("material.bump_map.coords.u_tiling = {0}; material.bump_map.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.bump_map.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.bump_map.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...

            bool isTranslucent = ((opacityMap != null) || (opacityMapAmount != 1));

            if (isTranslucent)
            {
                string addOpacityMapCommand = "";
                if (opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", opacityMap);
                }

                float opacityMapConstant = (int)(255f * opacityMapAmount);
                Commands.Add(string.Format("material.cutout_map = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (opacityMap != null)
                {
                    Commands.Add(string.Format("material.cutout_map.map1.coords.u_tiling = {0}; material.cutout_map.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                    if (DisableFiltering)
                    {
                        Commands.Add(string.Format("material.cutout_map.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.cutout_map.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.Reflectivity = {0}", specularLevel));
            Commands.Add(string.Format("material.Reflection_Glossiness = {0}", glossiness * GlossScalar));

            if (specularMap != null)
            {
                Commands.Add(string.Format("material.Reflection_Color_Map = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("material.Reflection_Color_Map.map1.coords.u_tiling = {0}; material.Reflection_Color_Map.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.Reflection_Color_Map.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.Reflection_Color_Map.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.Reflection_Color = (color {0} {1} {2})", specular.R, specular.G, specular.B));
            }

            if(AOEnable){
                Commands.Add("material.opts_ao_on = true");
                Commands.Add("material.opts_ao_use_global_ambient = true");
                Commands.Add("material.opts_ao_exact = true");
                Commands.Add("material.opts_ao_samples = 12");
                Commands.Add(string.Format("material.opts_ao_distance = {0}", AORayDistance));
            }

            if (EnableHighlightsFGOnly)
            {
                Commands.Add("material.refl_hlonly = true");
            }

            /*If a material is opaque, then flag it as such for mental ray to improve render times. The property in mental ray connection is that of the slot however and not the material, so we have to 'assign' the material,
             then make the change - http://forums.cgsociety.org/archive/index.php/t-914476.html*/
            if (!isTranslucent)
            {
                Commands.Add("m = meditmaterials[1]");
                Commands.Add("meditmaterials[1] = material");
                Commands.Add("material.mental_ray__material_custom_attribute.Opaque = true");
                Commands.Add("meditmaterials[1] = m");
            }

            Commands.Add("(getHandleByAnim material) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }

    public class MaterialOptionsVRayMaterial : IMaterialCreationOptions
    {
        public bool MapFilteringDisable { get; set; }
        public float GlossScalar { get; set; }
        public float BumpScalar { get; set; }

        public MaterialOptionsVRayMaterial()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            GlossScalar = Defaults.VRay_GlossScalar;
            BumpScalar = Defaults.VRay_BumpScalar;
        }

        public IMaxScriptMaterialProperties GetNewMaterial()
        {
            MaxScriptVRayMaterial material = new MaxScriptVRayMaterial();
            material.DisableFiltering = MapFilteringDisable;
            material.GlossScalar = GlossScalar;
            material.BumpScalar = BumpScalar;
            return material;
        }

        public string MaterialName
        {
            get { return "VRay Material"; }
        }

        public object BindingInfo { get; set; }
    }

    public class MaxScriptVRayMaterial : MaxScriptMaterial
    {
        public MaxScriptVRayMaterial()
        {
            Name = "VRayMaxScriptMaterial";
        }

        public bool DisableFiltering = Defaults.MapFilteringDisable;
        public float GlossScalar = Defaults.VRay_GlossScalar;
        public float BumpScalar = Defaults.VRay_BumpScalar;

        public override string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = VRayMtl name:(\"{0}\")", Name));

            Commands.Add(string.Format("material.showInViewport = {0}", showInViewport));

            Commands.Add(string.Format("material.texmap_diffuse_multiplier = {0}", diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.texmap_diffuse = CompositeTextureMap()"));
            Commands.Add(string.Format("material.texmap_diffuse.add()"));
            Commands.Add(string.Format("material.texmap_diffuse.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", diffuseMap);
            }

            Commands.Add(string.Format("material.texmap_diffuse.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, diffuse.R, diffuse.G, diffuse.B));
            
            /*becaus even if theres no texture we use the map slot to set the colour...*/
            Commands.Add(string.Format("material.texmap_diffuse_on = true"));

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.coords.u_tiling = {0}; material.texmap_diffuse.mapList[1].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", ambientMap);
            }

            Commands.Add(string.Format("material.texmap_diffuse.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, ambient.R * ambientMapAmount, ambient.G * ambientMapAmount, ambient.B * ambientMapAmount));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.coords.u_tiling = {0}; material.texmap_diffuse.mapList[2].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.texmap_diffuse.mask[2] = material.texmap_diffuse.mapList[1]"));

            if (bumpMap != null)
            {
                Commands.Add(string.Format("material.texmap_bump = (bitmapTexture filename:\"{0}\")", bumpMap));
                Commands.Add(string.Format("material.texmap_bump_on = true"));
                Commands.Add(string.Format("material.texmap_bump_multiplier = {0}", (bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("material.texmap_bump.coords.u_tiling = {0}; material.bump_map.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.texmap_bump.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_bump.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...
            if (opacityMap != null || opacityMapAmount != 1)
            {
                string addOpacityMapCommand = "";
                if (opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", opacityMap);

                }

                float opacityMapConstant = (int)(255f * opacityMapAmount);
                Commands.Add(string.Format("material.texmap_opacity = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));
                Commands.Add(string.Format("material.texmap_opacity_on = true"));

                if (opacityMap != null)
                {
                    Commands.Add(string.Format("material.texmap_opacity.map1.coords.u_tiling = {0}; material.texmap_opacity.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                    if (DisableFiltering)
                    {
                        Commands.Add(string.Format("material.texmap_opacity.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.texmap_opacity.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.reflection_glossiness = {0}", glossiness * GlossScalar));

            if (specularMap != null)
            {
                Commands.Add(string.Format("material.texmap_reflection = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("material.texmap_reflection.map1.coords.u_tiling = {0}; material.texmap_reflection.map1.coords.v_tiling = {1};", u_tiling, v_tiling));
                Commands.Add(string.Format("material.texmap_reflection_on = true"));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.texmap_reflection.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.texmap_reflection.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.reflection = (color {0} {1} {2})", specularLevel * specular.R, specularLevel * specular.G, specularLevel * specular.B));
            }


            Commands.Add("(getHandleByAnim material) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }

    public class MaterialOptionsStandardMaterial : IMaterialCreationOptions
    {
        public bool MapFilteringDisable { get; set; }
        public float GlossScalar { get; set; }
        public float BumpScalar { get; set; }

        public MaterialOptionsStandardMaterial()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            GlossScalar = Defaults.Standard_GlossScalar;
            BumpScalar = Defaults.Standard_BumpScalar;
        }

        public IMaxScriptMaterialProperties GetNewMaterial()
        {
            MaxScriptStandardMaterial material = new MaxScriptStandardMaterial();
            material.DisableFiltering = MapFilteringDisable;
            material.GlossScalar = GlossScalar;
            material.BumpScalar = BumpScalar;
            return material;
        }

        public string MaterialName
        {
            get { return "Autodesk 3DS Max Standard Material"; }
        }

        public object BindingInfo { get; set; }
    }

    public class MaxScriptStandardMaterial : MaxScriptMaterial
    {
        public MaxScriptStandardMaterial()
        {
            Name = "standardMaxScriptMaterial";
        }

        protected bool twoSided = true;
        protected bool adTextureLock = true;
        protected bool adLock = true;
        protected bool dsLock = true;
        public bool DisableFiltering = Defaults.MapFilteringDisable;
        public float GlossScalar = Defaults.Standard_GlossScalar;
        public float BumpScalar = Defaults.Standard_BumpScalar;

        public override string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = StandardMaterial name:(\"{0}\")", Name));

            Commands.Add(string.Format("material.twoSided = {0}", twoSided));
            Commands.Add(string.Format("material.showInViewport = {0}", showInViewport));
            Commands.Add(string.Format("material.adTextureLock = {0}", adTextureLock));
            Commands.Add(string.Format("material.adLock = {0}", adLock));
            Commands.Add(string.Format("material.dsLock = {0}", dsLock));

            Commands.Add(string.Format("material.diffuseMapEnable = true;"));
            Commands.Add(string.Format("material.diffuseMapAmount = {0}", diffuseMapAmount * 100f));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.diffuseMap = CompositeTextureMap()"));
            Commands.Add(string.Format("material.diffuseMap.add()"));
            Commands.Add(string.Format("material.diffuseMap.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", diffuseMap);
            }

            Commands.Add(string.Format("material.diffuseMap.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, diffuse.R, diffuse.G, diffuse.B));

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.coords.u_tiling = {0}; material.diffuseMap.mapList[1].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", ambientMap);
            }

            Commands.Add(string.Format("material.diffuseMap.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, ambient.R * ambientMapAmount, ambient.G * ambientMapAmount, ambient.B * ambientMapAmount));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.coords.u_tiling = {0}; material.diffuseMap.mapList[2].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.diffuseMap.mask[2] = material.diffuseMap.mapList[1]"));

            if (bumpMap != null)
            {
                Commands.Add(string.Format("material.bumpMapEnable = true;"));
                Commands.Add(string.Format("material.bumpMap = (bitmapTexture filename:\"{0}\")", bumpMap));
                Commands.Add(string.Format("material.bumpMapAmount = {0}", (bumpMapAmount * BumpScalar * 100f)));
                Commands.Add(string.Format("material.bumpMap.coords.u_tiling = {0}; material.bumpMap.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.bumpMap.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.bumpMap.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...

            bool isTranslucent = ((opacityMap != null) || (opacityMapAmount != 1));

            if (isTranslucent)
            {
                Commands.Add(string.Format("material.opacityMapEnable = true;"));

                string addOpacityMapCommand = "";
                if (opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", opacityMap);
                }

                float opacityMapConstant = (int)(255f * opacityMapAmount);
                Commands.Add(string.Format("material.opacityMap = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (opacityMap != null)
                {
                    Commands.Add(string.Format("material.opacityMap.map1.coords.u_tiling = {0}; material.opacityMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                    if (DisableFiltering)
                    {
                        Commands.Add(string.Format("material.opacityMap.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.opacityMap.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.specularLevel = {0}", specularLevel));
            Commands.Add(string.Format("material.glossiness = {0}", glossiness * GlossScalar));

            if (specularMap != null)
            {
                Commands.Add(string.Format("material.specularMapEnable = true;"));
                Commands.Add(string.Format("material.specularMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("material.specularMap.map1.coords.u_tiling = {0}; material.specularMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("material.specularMap.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.specularMap.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.specular = (color {0} {1} {2})", specular.R, specular.G, specular.B));
            }

            /*If a material is opaque, then flag it as such for mental ray to improve render times. The property in mental ray connection is that of the slot however and not the material, so we have to 'assign' the material,
             then make the change - http://forums.cgsociety.org/archive/index.php/t-914476.html*/
            if (!isTranslucent)
            {
                Commands.Add("m = meditmaterials[1]");
                Commands.Add("meditmaterials[1] = material");
                Commands.Add("material.mental_ray__material_custom_attribute.Opaque = true");
                Commands.Add("meditmaterials[1] = m");
            }

            Commands.Add("(getHandleByAnim material) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }
}
