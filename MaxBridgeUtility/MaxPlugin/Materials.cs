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
        IMtl CreateMaterial(MaterialWrapper m);
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

        public IEnumerable<MaterialWrapper> GetMaterials(MyMesh myMesh)
        {
            foreach (var myMat in myMesh.Materials)
            {
                yield return new MaterialWrapper(myMat);
            }
        }

        public IMultiMtl CreateMultiMaterial(IList<MaterialWrapper> Materials)
        {
            int NumberOfMaterialSlots = Materials.Max(Material => Material.MaterialIndex);

            IMultiMtl maxMaterial = globalInterface.NewDefaultMultiMtl;
            maxMaterial.SetNumSubMtls(NumberOfMaterialSlots);

            foreach (var myMat in Materials)
            {
                maxMaterial.SetSubMtlAndName(myMat.MaterialIndex, materialOptions.CreateMaterial(myMat), ref myMat.MaterialName);
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

    }

    /// <summary>
    /// The MaterialSource class provides an interface to the material described by a set of string properties from Daz, allowing it to be used in a stable form throughout and querying it for 
    /// information such as Opacity state
    /// </summary>
    public class MaterialWrapper
    {
        public MaterialWrapper(Material material)
        {
            this.source = material;
            this.MaterialName = source.MaterialName; //because we need to pass it by ref later on.
            Initialise();
        }

        protected Material source;


        public bool IsTransparent   { get { return (opacityMapAmount == 0); } }
        public bool Initialised     { get; protected set; }



        public string   MaterialName;
        public int      MaterialIndex  { get { return source.MaterialIndex; } }


        public bool     showInViewport = true;

        public Color    ambient = Color.Black;
        public string   ambientMap = null;
        public float    ambientMapAmount = 1.0f;

        public string   bumpMap = null;
        public float    bumpMapAmount = 1.0f;

        public Color    diffuse = Color.FromArgb(127, 127, 127);
        public string   diffuseMap = null;
        public float    diffuseMapAmount = 1.0f;

        public string   opacityMap = null;
        public float    opacityMapAmount = 1.0f;

        public Color    specular = Color.FromArgb(230, 230, 230);
        public string   specularMap = null;
        public float    specularLevel = 0.0f;
        public float    glossiness = 10.0f;

        public float    u_tiling = 1;
        public float    v_tiling = 1;


        public void Initialise()
        {
            ambient = Defaults.AmbientGammaCorrection ? ConvertColour(source.GetColorSafe("Ambient Color", ambient)) : source.GetColorSafe("Ambient Color", ambient);
            ambientMap = source.GetString("Ambient Color Map");
            ambientMapAmount = source.GetFloatSafe("Ambient Strength", ambientMapAmount);
            bumpMap = source.GetString("Bump Strength Map");
            diffuseMap = source.GetString("Color Map");
            diffuse = ConvertColour(source.GetColorSafe("Diffuse Color", diffuse));
            diffuseMapAmount = source.GetFloatSafe("Diffuse Strength", diffuseMapAmount);
            opacityMap = source.GetString("Opacity Map");
            opacityMapAmount = source.GetFloatSafe("Opacity Strength", opacityMapAmount);
            specular = ConvertColour(source.GetColorSafe("Specular Color", specular));
            specularMap = source.GetString("Specular Color Map");
            specularLevel = source.GetFloatSafe("Specular Strength", specularLevel);
            glossiness = source.GetFloatSafe("Glossiness", glossiness);
            u_tiling = source.GetFloatSafe("Horizontal Tiles", u_tiling);
            v_tiling = source.GetFloatSafe("Vertical Tiles", v_tiling);
            bumpMapAmount = ((source.GetFloatSafe(new string[] { "Positive Bump", "Bump Maximum" }, 0.1f) - source.GetFloatSafe(new string[] { "Negative Bump", "Bump Minimum" }, -0.1f)) * source.GetFloatSafe("Bump Strength", 1.0f));

            Initialised = true;
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

    public abstract class MaxScriptMaterialGenerator
    {
        protected IMtl GetFromScript(string script)
        {
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

        protected IAnimatable Convert(System.Int64 handle)
        {
            IAnimatable anim = GlobalInterface.Instance.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }
    }

    public class MaterialOptionsMentalRayArchAndDesign : MaxScriptMaterialGenerator, IMaterialCreationOptions
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

        public string MaterialName
        {
            get { return "MentalRay Arch & Design Material"; }
        }

        public object BindingInfo { get; set; }

        public IMtl CreateMaterial(MaterialWrapper m)
        {
            return GetFromScript(MakeScript(m));
        }

        public bool EnableHighlightsFGOnly = Defaults.MentalRay_EnableHighlightsFGOnly;

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = Arch___Design__mi name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("material.showInViewport = {0}", m.showInViewport));

            Commands.Add(string.Format("material.diffuse_weight = {0}", m.diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.Diffuse_Color_Map = CompositeTextureMap()"));
            Commands.Add(string.Format("material.Diffuse_Color_Map.add()"));
            Commands.Add(string.Format("material.Diffuse_Color_Map.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.coords.u_tiling = {0}; material.Diffuse_Color_Map.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.coords.u_tiling = {0}; material.Diffuse_Color_Map.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.Diffuse_Color_Map.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.Diffuse_Color_Map.mask[2] = material.Diffuse_Color_Map.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("material.bump_map = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("material.Bump_Map_Amount = {0}", (m.bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("material.bump_map.coords.u_tiling = {0}; material.bump_map.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.bump_map.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.bump_map.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...

            bool isTranslucent = ((m.opacityMap != null) || (m.opacityMapAmount != 1));

            if (isTranslucent)
            {
                string addOpacityMapCommand = "";
                if (m.opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.opacityMap);
                }

                float opacityMapConstant = (int)(255f * m.opacityMapAmount);
                Commands.Add(string.Format("material.cutout_map = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("material.cutout_map.map1.coords.u_tiling = {0}; material.cutout_map.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("material.cutout_map.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.cutout_map.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.Reflectivity = {0}", m.specularLevel));
            Commands.Add(string.Format("material.Reflection_Glossiness = {0}", m.glossiness * GlossScalar));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("material.Reflection_Color_Map = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("material.Reflection_Color_Map.map1.coords.u_tiling = {0}; material.Reflection_Color_Map.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.Reflection_Color_Map.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.Reflection_Color_Map.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.Reflection_Color = (color {0} {1} {2})", m.specular.R, m.specular.G, m.specular.B));
            }

            if (AOEnable)
            {
                Commands.Add("material.opts_ao_on = true");
                Commands.Add("material.opts_ao_use_global_ambient = true");
                Commands.Add("material.opts_ao_exact = true");
                Commands.Add("material.opts_ao_samples = 12");
                Commands.Add(string.Format("material.opts_ao_distance = {0}", AODistance));
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

    public class MaterialOptionsVRayMaterial : MaxScriptMaterialGenerator, IMaterialCreationOptions
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

        public IMtl CreateMaterial(MaterialWrapper m)
        {
            return GetFromScript(MakeScript(m));
        }

        public string MaterialName
        {
            get { return "VRay Material"; }
        }

        public object BindingInfo { get; set; }

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = VRayMtl name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("material.showInViewport = {0}", m.showInViewport));

            Commands.Add(string.Format("material.texmap_diffuse_multiplier = {0}", m.diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.texmap_diffuse = CompositeTextureMap()"));
            Commands.Add(string.Format("material.texmap_diffuse.add()"));
            Commands.Add(string.Format("material.texmap_diffuse.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("material.texmap_diffuse.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            /*becaus even if theres no texture we use the map slot to set the colour...*/
            Commands.Add(string.Format("material.texmap_diffuse_on = true"));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.coords.u_tiling = {0}; material.texmap_diffuse.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("material.texmap_diffuse.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.coords.u_tiling = {0}; material.texmap_diffuse.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_diffuse.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.texmap_diffuse.mask[2] = material.texmap_diffuse.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("material.texmap_bump = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("material.texmap_bump_on = true"));
                Commands.Add(string.Format("material.texmap_bump_multiplier = {0}", (m.bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("material.texmap_bump.coords.u_tiling = {0}; material.bump_map.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.texmap_bump.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.texmap_bump.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...
            if (m.opacityMap != null || m.opacityMapAmount != 1)
            {
                string addOpacityMapCommand = "";
                if (m.opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.opacityMap);

                }

                float opacityMapConstant = (int)(255f * m.opacityMapAmount);
                Commands.Add(string.Format("material.texmap_opacity = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));
                Commands.Add(string.Format("material.texmap_opacity_on = true"));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("material.texmap_opacity.map1.coords.u_tiling = {0}; material.texmap_opacity.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("material.texmap_opacity.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.texmap_opacity.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.reflection_glossiness = {0}", m.glossiness * GlossScalar));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("material.texmap_reflection = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("material.texmap_reflection.map1.coords.u_tiling = {0}; material.texmap_reflection.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));
                Commands.Add(string.Format("material.texmap_reflection_on = true"));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.texmap_reflection.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.texmap_reflection.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.reflection = (color {0} {1} {2})", m.specularLevel * m.specular.R, m.specularLevel * m.specular.G, m.specularLevel * m.specular.B));
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

    public class MaterialOptionsStandardMaterial : MaxScriptMaterialGenerator, IMaterialCreationOptions
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

        public IMtl CreateMaterial(MaterialWrapper m)
        {
            return GetFromScript(MakeScript(m));
        }

        public string MaterialName
        {
            get { return "Autodesk 3DS Max Standard Material"; }
        }

        public object BindingInfo { get; set; }

        protected bool twoSided = true;
        protected bool adTextureLock = true;
        protected bool adLock = true;
        protected bool dsLock = true;

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = StandardMaterial name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("material.twoSided = {0}", twoSided));
            Commands.Add(string.Format("material.showInViewport = {0}", m.showInViewport));
            Commands.Add(string.Format("material.adTextureLock = {0}", adTextureLock));
            Commands.Add(string.Format("material.adLock = {0}", adLock));
            Commands.Add(string.Format("material.dsLock = {0}", dsLock));

            Commands.Add(string.Format("material.diffuseMapEnable = true;"));
            Commands.Add(string.Format("material.diffuseMapAmount = {0}", m.diffuseMapAmount * 100f));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("material.diffuseMap = CompositeTextureMap()"));
            Commands.Add(string.Format("material.diffuseMap.add()"));
            Commands.Add(string.Format("material.diffuseMap.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("material.diffuseMap.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.coords.u_tiling = {0}; material.diffuseMap.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.diffuseMap.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("material.diffuseMap.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.coords.u_tiling = {0}; material.diffuseMap.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.diffuseMap.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("material.diffuseMap.mask[2] = material.diffuseMap.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("material.bumpMapEnable = true;"));
                Commands.Add(string.Format("material.bumpMap = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("material.bumpMapAmount = {0}", (m.bumpMapAmount * BumpScalar * 100f)));
                Commands.Add(string.Format("material.bumpMap.coords.u_tiling = {0}; material.bumpMap.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.bumpMap.coords.blur = 0.01;"));
                    Commands.Add(string.Format("material.bumpMap.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...

            bool isTranslucent = ((m.opacityMap != null) || (m.opacityMapAmount != 1));

            if (isTranslucent)
            {
                Commands.Add(string.Format("material.opacityMapEnable = true;"));

                string addOpacityMapCommand = "";
                if (m.opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.opacityMap);
                }

                float opacityMapConstant = (int)(255f * m.opacityMapAmount);
                Commands.Add(string.Format("material.opacityMap = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("material.opacityMap.map1.coords.u_tiling = {0}; material.opacityMap.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("material.opacityMap.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("material.opacityMap.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("material.specularLevel = {0}", m.specularLevel));
            Commands.Add(string.Format("material.glossiness = {0}", m.glossiness * GlossScalar));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("material.specularMapEnable = true;"));
                Commands.Add(string.Format("material.specularMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("material.specularMap.map1.coords.u_tiling = {0}; material.specularMap.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("material.specularMap.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("material.specularMap.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("material.specular = (color {0} {1} {2})", m.specular.R, m.specular.G, m.specular.B));
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
