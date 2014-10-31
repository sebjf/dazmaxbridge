using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public class MaterialOptionsMentalRayArchAndDesignSkinExperimental : MaterialOptionsMentalRayArchAndDesign, IMaterialCreator
    {
        /* This experimental material will identify skin materials and create an SSS2 Skin from them, based on a template material */

        [GuiProperty("Material Template", GuiPropertyAttribute.ControlTypeEnum.MaterialTemplateDropdown)]
        public IIMtlBaseView MaterialTemplate { get; set; }

        public MaterialOptionsMentalRayArchAndDesignSkinExperimental()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            BumpScalar = Defaults.MentalRaySkin_BumpScalar;
        }

        new public string MaterialName
        {
            get { return "MentalRay Arch & Design with Skin Experimental"; }
        }

        new public IMtl CreateMaterial(MaterialWrapper m)
        {
            switch (m.lightingModel)
            {
                case DzLightingModel.Skin:
                    return CreateSkinMaterial(m);
                default:
                    return base.CreateMaterial(m);

            }
        }

        protected IMtl CreateSkinMaterial(MaterialWrapper m)
        {
            IMtlBase referenceMtl = MaterialTemplate.GetMaterial(m);

            if (referenceMtl == null)
            {
                referenceMtl = intrface.CreateInstance(SClass_ID.Material, gi.Class_ID.Create(2004030991, 2251076473)) as IMtlBase;
            }

            if (!referenceMtl.ClassID.EqualsClassID(2004030991, 2251076473))
            {
                referenceMtl = intrface.CreateInstance(SClass_ID.Material, gi.Class_ID.Create(2004030991, 2251076473)) as IMtlBase;
            }

            // composite #(640, 0)

            ///    IMultiTex composite = intrface.CreateInstance(SClass_ID.Texmap, gi.Class_ID.Create(640, 0)) as IMultiTex;
            ///    
            IMultiTex composite = gi.NewDefaultCompositeTex;

            composite.InvokeMethod("add");

            List<Parameter> ps1 = composite.EnumerateProperties().ToList();

            composite.SetNumSubTexmaps(2);

            //gi.ClassDirectory.Instance.FindClass(composite.SuperClassID, composite.ClassID).NumInterfaces

            List<Parameter> ps2 = composite.EnumerateProperties().ToList();

            IMtlBase mtl = gi.CloneRefHierarchy(referenceMtl) as IMtlBase;
            mtl.Name = m.MaterialName;

            LoadMap(m, m.diffuseMap);

            return mtl as IMtl;
        }

        IBitmapTex LoadMap(MaterialWrapper m, string filename)
        {
            IBitmapTex bmp = gi.NewDefaultBitmapTex;

            bmp.SetMapName(filename, false);

            bmp.FindPropertyByName("coords").FindPropertyByName("u_tiling").SetValue(m.u_tiling);
            bmp.FindPropertyByName("coords").FindPropertyByName("v_tiling").SetValue(m.v_tiling);

            if (MapFilteringDisable)
            {
                bmp.FindPropertyByName("coords").FindPropertyByName("blur").SetValue(0.01f);
                bmp.FindPropertyByName("filtering").SetValue(2);
            }

            List<Parameter> p = bmp.EnumerateProperties().ToList();

            return bmp;
        }
    }

    public class MaterialOptionsMentalRayArchAndDesignSkin : MaterialOptionsMentalRayArchAndDesign, IMaterialCreator
    {
        /* This experimental material will identify skin materials and create an SSS2 Skin from them, based on a template material */

        [GuiProperty("Material Template [Skin]", GuiPropertyAttribute.ControlTypeEnum.MaterialTemplateDropdown)]
        public IIMtlBaseView MaterialTemplate { get; set; }

        [GuiProperty("Material Template [Matte]", GuiPropertyAttribute.ControlTypeEnum.MaterialTemplateDropdown)]
        public IIMtlBaseView MaterialTemplate2 { get; set; }

        [GuiProperty("Material Template [Glossy]", GuiPropertyAttribute.ControlTypeEnum.MaterialTemplateDropdown)]
        public IIMtlBaseView MaterialTemplate3 { get; set; }

        public MaterialOptionsMentalRayArchAndDesignSkin()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            BumpScalar = Defaults.MentalRaySkin_BumpScalar;
        }

        new public string MaterialName
        {
            get { return "MentalRay Arch & Design with Skin support"; }
        }

        new public IMtl CreateMaterial(MaterialWrapper m)
        {
            switch (m.lightingModel)
            {
                case DzLightingModel.Skin:
                    return CreateSkinMaterial(m, MaterialTemplate.GetMaterial(m));
                case DzLightingModel.Matte:
                    return CreateSkinMaterial(m, MaterialTemplate2.GetMaterial(m));
                case DzLightingModel.GlossyPlastic:
                    return CreateSkinMaterial(m, MaterialTemplate3.GetMaterial(m));
                default:
                    return base.CreateMaterial(m);
            }
        }

        protected IMtl CreateSkinMaterial(MaterialWrapper m, IMtlBase referenceMtl)
        {
            if (referenceMtl == null)
            {
                referenceMtl = intrface.CreateInstance(SClass_ID.Material, gi.Class_ID.Create(2004030991, 2251076473)) as IMtlBase;
            }

            var script = MakeScript(m, referenceMtl);
            Log.Add(string.Format("Running script: {0}", script), LogLevel.Debug);
            return GetFromScript(script);
        }

        private List<String> LoadMap(string mapName, string filename, MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("{0} = (bitmapTexture filename:\"{1}\")", mapName, filename));
            Commands.Add(string.Format("{0}.coords.u_tiling = {1}; {0}.coords.v_tiling = {2};", mapName, m.u_tiling, m.v_tiling));

            if (MapFilteringDisable)
            {
                Commands.Add(string.Format("{0}.coords.blur = 0.01;", mapName));
                Commands.Add(string.Format("{0}.filtering = 2;", mapName));
            }

            return Commands;
        }

        private string RGBMultiplyMapAssignment(string nodeName, string mapExpression)
        {
            return string.Format("(for map_i in (getclassinstances RGB_Multiply target:mtl) where (map_i.name == \"{0}\") do (map_i.map1 = {1}))", nodeName, mapExpression);
        }

        protected string MakeScript(MaterialWrapper m, IMtlBase referenceMtl)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("mtl = copy (getAnimByHandle {0})", Convert(referenceMtl)));

            Commands.Add(string.Format("mtl.name = \"{0}\"", m.MaterialName));

            /*Build diffuse map*/

            Commands.Add(string.Format("diffuse_map = CompositeTextureMap()"));
            Commands.Add(string.Format("diffuse_map.add()"));
            Commands.Add(string.Format("diffuse_map.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("diffuse_map.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("diffuse_map.mapList[1].map1.coords.u_tiling = {0}; diffuse_map.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("diffuse_map.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("diffuse_map.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("diffuse_map.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("diffuse_map.mapList[2].map1.coords.u_tiling = {0}; diffuse_map.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("diffuse_map.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("diffuse_map.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("diffuse_map.mask[2] = diffuse_map.mapList[1]"));

            /*Apply diffuse map - if there is an RGB_Multiply called Diffuse, find it and set the map, otherwise leave it*/

            Commands.Add(RGBMultiplyMapAssignment("Diffuse", "diffuse_map"));

            if (m.diffuseMap != null)
            {
                Commands.Add("(if (diffuse_map.mapList[1].map1 != undefined) then (showTextureMap mtl diffuse_map.mapList[1].map1 on))"); //show the diffuse map in the viewport
            }

            /* Add the other maps if they are present */

            if (m.bumpMap != null)
            {
                Commands.AddRange(LoadMap("bump_map", m.bumpMap, m));
                Commands.Add(RGBMultiplyMapAssignment("Bump", "bump_map"));
            }

            if (m.opacityMap != null)
            {
                Commands.AddRange(LoadMap("opacity_map", m.opacityMap, m));
                Commands.Add(RGBMultiplyMapAssignment("Opacity", "opacity_map"));
            }

            if (m.specularMap != null)
            {
                Commands.AddRange(LoadMap("specular_map", m.specularMap, m));
                Commands.Add(RGBMultiplyMapAssignment("Specular", "specular_map"));
            }

            Commands.Add("(getHandleByAnim mtl) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.
        }


    }

    public class MaterialOptionsMentalRayArchAndDesign : MaxScriptMaterialGenerator, IMaterialCreator
    {
        /* Here are the options that should be exposed through the UI - they will be databound to controls on the form when this option is selected in the drop down list */

        [GuiProperty("Disable Map Filtering", GuiPropertyAttribute.ControlTypeEnum.Checkbox)]
        public bool MapFilteringDisable { get; set; }

        [GuiProperty("Ambient Occlusion Enable", GuiPropertyAttribute.ControlTypeEnum.Checkbox)]
        public bool AOEnable { get; set; }

        [GuiProperty("Ambient Occlusion Distance", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
        public int AODistance { get; set; }

        [GuiProperty("Bump Scalar", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
        public float BumpScalar { get; set; }

        public MaterialOptionsMentalRayArchAndDesign()
        {
            MapFilteringDisable = Defaults.MapFilteringDisable;
            AOEnable = Defaults.MentalRay_AOEnable;
            AODistance = Defaults.MentalRay_AODistance;
            BumpScalar = Defaults.MentalRay_BumpScalar;
        }

        public string MaterialName
        {
            get { return "MentalRay Arch & Design Material"; }
        }

        public object GuiControlCache { get; set; }

        public IMtl CreateMaterial(MaterialWrapper m)
        {
            return GetFromScript(MakeScript(m));
        }

        public bool EnableHighlightsFGOnly = Defaults.MentalRay_EnableHighlightsFGOnly;

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("mtl = Arch___Design__mi name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("mtl.showInViewport = {0}", m.showInViewport));

            Commands.Add(string.Format("mtl.diffuse_weight = {0}", m.diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("mtl.Diffuse_Color_Map = CompositeTextureMap()"));
            Commands.Add(string.Format("mtl.Diffuse_Color_Map.add()"));
            Commands.Add(string.Format("mtl.Diffuse_Color_Map.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[1].map1.coords.u_tiling = {0}; mtl.Diffuse_Color_Map.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[2].map1.coords.u_tiling = {0}; mtl.Diffuse_Color_Map.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.Diffuse_Color_Map.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("mtl.Diffuse_Color_Map.mask[2] = mtl.Diffuse_Color_Map.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("mtl.bump_map = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("mtl.Bump_Map_Amount = {0}", (m.bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("mtl.bump_map.coords.u_tiling = {0}; mtl.bump_map.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.bump_map.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.bump_map.filtering = 2;"));
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
                Commands.Add(string.Format("mtl.cutout_map = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("mtl.cutout_map.map1.coords.u_tiling = {0}; mtl.cutout_map.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("mtl.cutout_map.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("mtl.cutout_map.map1.filtering = 2;"));
                    }
                }
            }

            SpecularProperties specular = CorrectGlossiness(m.glossiness);

            Commands.Add(string.Format("mtl.Reflectivity = {0}", specular.Reflectivity * m.specularLevel));
            Commands.Add(string.Format("mtl.Reflection_Glossiness = {0}", specular.Glossiness));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("mtl.Reflection_Color_Map = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("mtl.Reflection_Color_Map.map1.coords.u_tiling = {0}; mtl.Reflection_Color_Map.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.Reflection_Color_Map.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("mtl.Reflection_Color_Map.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("mtl.Reflection_Color = (color {0} {1} {2})", m.specular.R, m.specular.G, m.specular.B));
            }

            if (AOEnable)
            {
                Commands.Add("mtl.opts_ao_on = true");
                Commands.Add("mtl.opts_ao_use_global_ambient = true");
                Commands.Add("mtl.opts_ao_exact = true");
                Commands.Add("mtl.opts_ao_samples = 12");
                Commands.Add(string.Format("mtl.opts_ao_distance = {0}", AODistance));
            }

            if (EnableHighlightsFGOnly)
            {
                Commands.Add("mtl.refl_hlonly = true");
            }

            /*If a material is opaque, then flag it as such for mental ray to improve render times. The property in mental ray connection is that of the slot however and not the material, so we have to 'assign' the material,
             then make the change - http://forums.cgsociety.org/archive/index.php/t-914476.html*/
            if (!isTranslucent)
            {
                Commands.Add("m = meditmaterials[1]");
                Commands.Add("meditmaterials[1] = mtl");
                Commands.Add("mtl.mental_ray__material_custom_attribute.Opaque = true");
                Commands.Add("meditmaterials[1] = m");
            }

            Commands.Add("(getHandleByAnim mtl) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.
        }

        #region Specular Correction

        /* This function was calibrated by manually adjusting mental ray Arch & Design material properties to match renders from Daz, which is why it is in
         * MaxScriptMaterialGenerator. See the spreadsheet for the empirically matched values. */

        protected struct SpecularProperties
        {
            public float Glossiness;
            public float Reflectivity;
        }

        protected SpecularProperties CorrectGlossiness(float glossiness)
        {
            int index = glossiness > 0 ? ((int)(glossiness * 10)) / 10 : 0;
            return new SpecularProperties() { Reflectivity = reflectivityValues[index], Glossiness = glossinessValues[index] };
        }

        protected float[] dazGlossinessValues = { 0.0f, 0.10f, 0.20f, 0.30f, 0.40f, 0.50f, 0.60f, 0.70f, 0.80f, 0.90f, 1.0f};
        protected float[] reflectivityValues = { 1.0f, 0.9f, 0.6f, 0.5f, 0.3f, 0.3f, 0.2f, 0.2f, 0.1f, 0.05f, 0.01f };
        protected float[] glossinessValues = { 0.1f, 0.2f, 0.3f, 0.4f, 0.45f, 0.45f, 0.5f, 0.6f, 0.7f, 0.9f, 1.0f };

        #endregion
    }

    public class MaterialOptionsVRayMaterial : MaxScriptMaterialGenerator, IMaterialCreator
    {
        [GuiProperty("Disable Map Filtering", GuiPropertyAttribute.ControlTypeEnum.Checkbox)]
        public bool MapFilteringDisable { get; set; }

        [GuiProperty("Gloss Scalar", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
        public float GlossScalar { get; set; }

        [GuiProperty("Bump Scalar", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
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

        public object GuiControlCache { get; set; }

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("material = VRayMtl name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("mtl.showInViewport = {0}", m.showInViewport));

            Commands.Add(string.Format("mtl.texmap_diffuse_multiplier = {0}", m.diffuseMapAmount));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("mtl.texmap_diffuse = CompositeTextureMap()"));
            Commands.Add(string.Format("mtl.texmap_diffuse.add()"));
            Commands.Add(string.Format("mtl.texmap_diffuse.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("mtl.texmap_diffuse.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            /*becaus even if theres no texture we use the map slot to set the colour...*/
            Commands.Add(string.Format("mtl.texmap_diffuse_on = true"));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("mtl.texmap_diffuse.mapList[1].map1.coords.u_tiling = {0}; mtl.texmap_diffuse.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.texmap_diffuse.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.texmap_diffuse.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("mtl.texmap_diffuse.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("mtl.texmap_diffuse.mapList[2].map1.coords.u_tiling = {0}; mtl.texmap_diffuse.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.texmap_diffuse.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.texmap_diffuse.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("mtl.texmap_diffuse.mask[2] = mtl.texmap_diffuse.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("mtl.texmap_bump = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("mtl.texmap_bump_on = true"));
                Commands.Add(string.Format("mtl.texmap_bump_multiplier = {0}", (m.bumpMapAmount * BumpScalar)));
                Commands.Add(string.Format("mtl.texmap_bump.coords.u_tiling = {0}; mtl.bump_map.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.texmap_bump.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.texmap_bump.filtering = 2;"));
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
                Commands.Add(string.Format("mtl.texmap_opacity = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));
                Commands.Add(string.Format("mtl.texmap_opacity_on = true"));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("mtl.texmap_opacity.map1.coords.u_tiling = {0}; mtl.texmap_opacity.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("mtl.texmap_opacity.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("mtl.texmap_opacity.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("mtl.reflection_glossiness = {0}", m.glossiness * GlossScalar));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("mtl.texmap_reflection = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("mtl.texmap_reflection.map1.coords.u_tiling = {0}; mtl.texmap_reflection.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));
                Commands.Add(string.Format("mtl.texmap_reflection_on = true"));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.texmap_reflection.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("mtl.texmap_reflection.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("mtl.reflection = (color {0} {1} {2})", m.specularLevel * m.specular.R, m.specularLevel * m.specular.G, m.specularLevel * m.specular.B));
            }


            Commands.Add("(getHandleByAnim mtl) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.
        }
    }

    public class MaterialOptionsStandardMaterial : MaxScriptMaterialGenerator, IMaterialCreator
    {
        [GuiProperty("Disable Map Filtering", GuiPropertyAttribute.ControlTypeEnum.Checkbox)]
        public bool MapFilteringDisable { get; set; }

        [GuiProperty("Gloss Scalar", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
        public float GlossScalar { get; set; }

        [GuiProperty("Bump Scalar", GuiPropertyAttribute.ControlTypeEnum.Textbox)]
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

        public object GuiControlCache { get; set; }

        protected bool twoSided = true;
        protected bool adTextureLock = true;
        protected bool adLock = true;
        protected bool dsLock = true;

        protected string MakeScript(MaterialWrapper m)
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("mtl = StandardMaterial name:(\"{0}\")", m.MaterialName));

            Commands.Add(string.Format("mtl.twoSided = {0}", twoSided));
            Commands.Add(string.Format("mtl.showInViewport = {0}", m.showInViewport));
            Commands.Add(string.Format("mtl.adTextureLock = {0}", adTextureLock));
            Commands.Add(string.Format("mtl.adLock = {0}", adLock));
            Commands.Add(string.Format("mtl.dsLock = {0}", dsLock));

            Commands.Add(string.Format("mtl.diffuseMapEnable = true;"));
            Commands.Add(string.Format("mtl.diffuseMapAmount = {0}", m.diffuseMapAmount * 100f));

            // CompositeTextureMap help page: http://docs.autodesk.com/3DSMAX/15/ENU/MAXScript-Help/index.html?url=files/GUID-611E1342-F976-4E95-8F78-88175B329745.htm,topicNumber=d30e510982
            Commands.Add(string.Format("mtl.diffuseMap = CompositeTextureMap()"));
            Commands.Add(string.Format("mtl.diffuseMap.add()"));
            Commands.Add(string.Format("mtl.diffuseMap.blendMode[2] = 2"));

            string addDiffuseMapCommand = "";
            if (m.diffuseMap != null)
            {
                addDiffuseMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.diffuseMap);
            }

            Commands.Add(string.Format("mtl.diffuseMap.mapList[1] = RGB_Multiply {0} color2:(color {1} {2} {3})", addDiffuseMapCommand, m.diffuse.R, m.diffuse.G, m.diffuse.B));

            if (m.diffuseMap != null)
            {
                Commands.Add(string.Format("mtl.diffuseMap.mapList[1].map1.coords.u_tiling = {0}; mtl.diffuseMap.mapList[1].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.diffuseMap.mapList[1].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.diffuseMap.mapList[1].map1.filtering = 2;"));
                }
            }

            string addAmbientMapCommand = "";
            if (m.ambientMap != null)
            {
                addAmbientMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.ambientMap);
            }

            Commands.Add(string.Format("mtl.diffuseMap.mapList[2] = RGB_Multiply {0} color2:(color {1} {2} {3})", addAmbientMapCommand, m.ambient.R * m.ambientMapAmount, m.ambient.G * m.ambientMapAmount, m.ambient.B * m.ambientMapAmount));

            if (m.ambientMap != null)
            {
                Commands.Add(string.Format("mtl.diffuseMap.mapList[2].map1.coords.u_tiling = {0}; mtl.diffuseMap.mapList[2].map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.diffuseMap.mapList[2].map1.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.diffuseMap.mapList[2].map1.filtering = 2;"));
                }
            }

            Commands.Add(string.Format("mtl.diffuseMap.mask[2] = mtl.diffuseMap.mapList[1]"));

            if (m.bumpMap != null)
            {
                Commands.Add(string.Format("mtl.bumpMapEnable = true;"));
                Commands.Add(string.Format("mtl.bumpMap = (bitmapTexture filename:\"{0}\")", m.bumpMap));
                Commands.Add(string.Format("mtl.bumpMapAmount = {0}", (m.bumpMapAmount * BumpScalar * 100f)));
                Commands.Add(string.Format("mtl.bumpMap.coords.u_tiling = {0}; mtl.bumpMap.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.bumpMap.coords.blur = 0.01;"));
                    Commands.Add(string.Format("mtl.bumpMap.filtering = 2;"));
                }
            }

            //If a cutout map is set, it marks all items as translucent to max which makes multiple pass alpha blending tricky, meaning that while the renders look ok, in the viewports it looks like the z-buffer
            //is being completely ignored, so, if there isn't actually any translucency/transparency, just dont do anything...

            bool isTranslucent = ((m.opacityMap != null) || (m.opacityMapAmount != 1));

            if (isTranslucent)
            {
                Commands.Add(string.Format("mtl.opacityMapEnable = true;"));

                string addOpacityMapCommand = "";
                if (m.opacityMap != null)
                {
                    addOpacityMapCommand = string.Format("map1:(bitmapTexture filename:\"{0}\")", m.opacityMap);
                }

                float opacityMapConstant = (int)(255f * m.opacityMapAmount);
                Commands.Add(string.Format("mtl.opacityMap = RGB_Multiply {0} color2:(color {1} {2} {3})", addOpacityMapCommand, opacityMapConstant, opacityMapConstant, opacityMapConstant));

                if (m.opacityMap != null)
                {
                    Commands.Add(string.Format("mtl.opacityMap.map1.coords.u_tiling = {0}; mtl.opacityMap.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                    if (MapFilteringDisable)
                    {
                        Commands.Add(string.Format("mtl.opacityMap.map1.coords.blur = 0.01;"));
                        Commands.Add(string.Format("mtl.opacityMap.map1.filtering = 2;"));
                    }
                }
            }

            Commands.Add(string.Format("mtl.specularLevel = {0}", m.specularLevel));
            Commands.Add(string.Format("mtl.glossiness = {0}", m.glossiness * GlossScalar));

            if (m.specularMap != null)
            {
                Commands.Add(string.Format("mtl.specularMapEnable = true;"));
                Commands.Add(string.Format("mtl.specularMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", m.specularMap, m.specular.R, m.specular.G, m.specular.B));
                Commands.Add(string.Format("mtl.specularMap.map1.coords.u_tiling = {0}; mtl.specularMap.map1.coords.v_tiling = {1};", m.u_tiling, m.v_tiling));

                if (MapFilteringDisable)
                {
                    Commands.Add(string.Format("mtl.specularMap.map1.coords.blur = 0.01"));
                    Commands.Add(string.Format("mtl.specularMap.map1.filtering = 2"));
                }
            }
            else
            {
                Commands.Add(string.Format("mtl.specular = (color {0} {1} {2})", m.specular.R, m.specular.G, m.specular.B));
            }

            /*If a material is opaque, then flag it as such for mental ray to improve render times. The property in mental ray connection is that of the slot however and not the material, so we have to 'assign' the material,
             then make the change - http://forums.cgsociety.org/archive/index.php/t-914476.html*/
            if (!isTranslucent)
            {
                Commands.Add("m = meditmaterials[1]");
                Commands.Add("meditmaterials[1] = mtl");
                Commands.Add("mtl.mental_ray__material_custom_attribute.Opaque = true");
                Commands.Add("meditmaterials[1] = m");
            }

            Commands.Add("(getHandleByAnim mtl) as String");

            string script = "";
            foreach (var c in Commands)
            {
                script += (c + "; ");
            }
            return "(" + script + ")"; //Note, remove these brackets to have max print the results of each command in the set when debugging.

        }
    }
}
