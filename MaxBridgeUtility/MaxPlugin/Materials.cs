﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using Autodesk.Max;
using System.Windows.Forms;

namespace MaxManagedBridge
{


    public partial class MaxPlugin : MaxBridge
    {
        public string[] AvailableMaterials = { "mental ray Arch & Design", "3DS Max Standard", "VRay" };
        public int SelectedMaterial = 0;

        public float BumpScalar = 500.0f;
        public float GlossinessScalar = 0.4f;

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
                final += (s + "\n");
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
            IMaxScriptMaterialProperties maxMaterial;

            switch (SelectedMaterial)
            {
                case 1:
                    maxMaterial = new MaxScriptStandardMaterial();
                    break;
                case 2:
                    maxMaterial = new MaxScriptVRayMaterial();
                    break;
                default:
                case 0:
                    maxMaterial = new MaxScriptMentalRayArchAndDesignMaterial();
                    break;
            };

            /* All percentages should be between 0-1 and any conversions done in the script generation */

            maxMaterial.Name = material.MaterialName;
            maxMaterial.Ambient = ConvertColour(material.GetColor("Ambient Color"));
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
            maxMaterial.Glossiness = material.GetFloat("Glossiness") * GlossinessScalar;
            maxMaterial.U_tiling = material.GetFloat("Horizontal Tiles");
            maxMaterial.V_tiling = material.GetFloat("Vertical Tiles");

            maxMaterial.BumpMapAmount = (material.GetFloatSafe("Positive Bump", 0) - material.GetFloatSafe("Negative Bump", 0)) * material.GetFloatSafe("Bump Strength", 0.03f) * BumpScalar;

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
            catch (Exception e)
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
        bool DisableFiltering { set; }

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

        public bool DisableFiltering { get; set; }

        public bool twoSided = true;
        public bool showInViewport = true;
        public bool adTextureLock = true;
        public bool adLock = true;
        public bool dsLock = true;

        protected Color ambient = Color.Black;
        protected string ambientMap = null;
        protected float ambientMapAmount = 1.0f;

        protected string bumpMap = null;
        protected float bumpMapAmount = 100.0f;

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

    public class MaxScriptMentalRayArchAndDesignMaterial : MaxScriptMaterial
    {
        public MaxScriptMentalRayArchAndDesignMaterial()
        {
            Name = "mrArchDesignMaxScriptMaterial";
            DisableFiltering = true;
        }

        public bool EnableAO = true;
        public bool EnableHighlightsFGOnly = true;

        private const float BumpModifier = 0.5f; //this is to bring it in line with BumpScalar above, not to control it.

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
                Commands.Add(string.Format("material.Bump_Map_Amount = {0}", (bumpMapAmount * 0.01 * BumpModifier)));
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
            Commands.Add(string.Format("material.Reflection_Glossiness = {0}", glossiness));

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

            if(EnableAO){
                Commands.Add("material.opts_ao_on = true");
                Commands.Add("material.opts_ao_use_global_ambient = true");
                Commands.Add("material.opts_ao_exact = false");
                Commands.Add("material.opts_ao_samples = 12");
                Commands.Add("material.opts_ao_distance = 400");
            }

            if (EnableHighlightsFGOnly)
            {
                Commands.Add("material.refl_hlonly = true");
            }

            /*If a material is opaque, then flag it as so for mental ray to improve render times. The property in mental ray connection is that of the slot however and not the material, so we have to 'assign' the material,
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

    public class MaxScriptVRayMaterial : MaxScriptMaterial
    {
        public MaxScriptVRayMaterial()
        {
            Name = "VRayMaxScriptMaterial";
            DisableFiltering = true;
        }

        private const float BumpModifier = 0.5f; //this is to bring it in line with BumpScalar above, not to control it.

        public override string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("vmaterial = VRayMtl name:(\"{0}\")", Name));

            Commands.Add(string.Format("vmaterial.showInViewport = {0}", showInViewport));

            Commands.Add(string.Format("vmaterial.texmap_diffuse_multiplier = {0}", diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("vmaterial.texmap_diffuse = CompositeTextureMap()"));
            Commands.Add(string.Format("vmaterial.texmap_diffuse.add()"));
            Commands.Add(string.Format("vmaterial.texmap_diffuse.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", diffuseMap);
            }

            Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, diffuse.R, diffuse.G, diffuse.B));
            
            /*becaus even if theres no texture we use the map slot to set the colour...*/
            Commands.Add(string.Format("vmaterial.texmap_diffuse_on = true"));

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[1].map1.coords.u_tiling = {0}; vmaterial.texmap_diffuse.mapList[1].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", ambientMap);
            }

            Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, ambient.R * ambientMapAmount, ambient.G * ambientMapAmount, ambient.B * ambientMapAmount));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[2].map1.coords.u_tiling = {0}; vmaterial.texmap_diffuse.mapList[2].map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("vmaterial.texmap_diffuse.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("vmaterial.texmap_diffuse.mask[2] = vmaterial.texmap_diffuse.mapList[1]"));

            if (bumpMap != null)
            {
                Commands.Add(string.Format("vmaterial.texmap_bump = (bitmapTexture filename:\"{0}\")", bumpMap));
                Commands.Add(string.Format("vmaterial.texmap_bump_on = true"));
                Commands.Add(string.Format("vmaterial.texmap_bump_multiplier = {0}", (bumpMapAmount * 0.01 * BumpModifier)));
                Commands.Add(string.Format("vmaterial.texmap_bump.coords.u_tiling = {0}; vmaterial.bump_map.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("vmaterial.texmap_bump.coords.blur = 0.01;"));
                    Commands.Add(string.Format("vmaterial.texmap_bump.filtering = 2;"));
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
                Commands.Add(string.Format("vmaterial.texmap_opacity = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));
                Commands.Add(string.Format("vmaterial.texmap_opacity_on = true"));

                if (opacityMap != null)
                {
                    Commands.Add(string.Format("vmaterial.texmap_opacity.map1.coords.u_tiling = {0}; vmaterial.texmap_opacity.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                    if (DisableFiltering)
                    {
                        Commands.Add(string.Format("vmaterial.texmap_opacity.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("vmaterial.texmap_opacity.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("vmaterial.reflection_glossiness = {0}", glossiness));

            if (specularMap != null)
            {
                Commands.Add(string.Format("vmaterial.texmap_reflection = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("vmaterial.texmap_reflection.map1.coords.u_tiling = {0}; vmaterial.texmap_reflection.map1.coords.v_tiling = {1};", u_tiling, v_tiling));
                Commands.Add(string.Format("vmaterial.texmap_reflection_on = true"));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("vmaterial.texmap_reflection.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("vmaterial.texmap_reflection.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("vmaterial.reflection = (color {0} {1} {2})", specularLevel * specular.R, specularLevel * specular.G, specularLevel * specular.B));
            }


            Commands.Add("(getHandleByAnim vmaterial) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }

    public class MaxScriptStandardMaterial : MaxScriptMaterial
    {
        public MaxScriptStandardMaterial()
        {
            Name = "standardMaxScriptMaterial";
            DisableFiltering = true;
        }

        public override string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("stdmaterial = StandardMaterial name:(\"{0}\")", Name));

            Commands.Add(string.Format("stdmaterial.twoSided = {0}", twoSided));
            Commands.Add(string.Format("stdmaterial.showInViewport = {0}", showInViewport));
            Commands.Add(string.Format("stdmaterial.adTextureLock = {0}", adTextureLock));
            Commands.Add(string.Format("stdmaterial.adLock = {0}", adLock));
            Commands.Add(string.Format("stdmaterial.dsLock = {0}", dsLock));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("stdmaterial.ambientMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", ambientMap, ambient.R, ambient.G, ambient.B));
                Commands.Add(string.Format("stdmaterial.ambientMapAmount = {0}", ambientMapAmount * 100.0));
                Commands.Add(string.Format("stdmaterial.ambientMap.map1.coords.u_tiling = {0}; stdmaterial.ambientMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("stdmaterial.ambientMap.map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("stdmaterial.ambientMap.map1.filtering = 2;"));
                }
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.ambient = color {0} {1} {2}", ambient.R, ambient.G, ambient.B));
            }

            if (bumpMap != null)
            {
                Commands.Add(string.Format("stdmaterial.bumpMap = (bitmapTexture filename:\"{0}\")", bumpMap));
                Commands.Add(string.Format("stdmaterial.bumpMapAmount = {0}", bumpMapAmount));
                Commands.Add(string.Format("stdmaterial.bumpMap.coords.u_tiling = {0}; stdmaterial.bumpMap.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("stdmaterial.bumpMap.coords.blur = 0.01;"));
                    Commands.Add(string.Format("stdmaterial.bumpMap.filtering = 2;"));
                }
            }

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("stdmaterial.diffuseMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", diffuseMap, diffuse.R, diffuse.G, diffuse.B));
                Commands.Add(string.Format("stdmaterial.diffuseMapAmount = {0}", diffuseMapAmount * 100.0));
                Commands.Add(string.Format("stdmaterial.diffuseMap.map1.coords.u_tiling = {0}; stdmaterial.diffuseMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("stdmaterial.diffuseMap.map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("stdmaterial.diffuseMap.map1.filtering = 2;"));
                }
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.diffuse = color {0} {1} {2}", diffuse.R, diffuse.G, diffuse.B));
            }

            if (opacityMap != null)
            {
                Commands.Add(string.Format("stdmaterial.opacityMap = (bitmapTexture filename:\"{0}\")", opacityMap));
                Commands.Add(string.Format("stdmaterial.opacityMapAmount = {0}", opacityMapAmount * 100.0));
                Commands.Add(string.Format("stdmaterial.opacityMap.coords.u_tiling = {0}; stdmaterial.opacityMap.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("stdmaterial.opacityMap.coords.blur = 0.01;"));
                    Commands.Add(string.Format("stdmaterial.opacityMap.filtering = 2;"));
                }
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.opacity = {0}", opacityMapAmount * 100.0));
            }

            if (specularMap != null)
            {
                Commands.Add(string.Format("stdmaterial.specularMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("stdmaterial.specularMap.map1.coords.u_tiling = {0}; stdmaterial.specularMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));

                if (DisableFiltering)
                {
                    Commands.Add(string.Format("stdmaterial.specularMap.map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("stdmaterial.specularMap.map1.filtering = 2;"));
                }
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.specular = (color {0} {1} {2})", specular.R, specular.G, specular.B));
            }

            Commands.Add(string.Format("stdmaterial.specularLevel = {0}", specularLevel * 100.0));
            Commands.Add(string.Format("stdmaterial.glossiness = {0}", glossiness * 100.0));

            Commands.Add("(getHandleByAnim stdmaterial) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }
}
