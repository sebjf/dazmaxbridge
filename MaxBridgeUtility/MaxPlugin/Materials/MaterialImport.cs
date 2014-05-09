using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public interface IMaterialCreator
    {
        IMtl CreateMaterial(MaterialWrapper m);
        string MaterialName { get; }
        object BindingInfo { get; set; } //This is for use by the GUI, don't touch it
    }

    public partial class MaxPlugin : MaxBridge
    {
        public readonly IMaterialCreator[] AvailableMaterialCreators = { new MaterialOptionsMentalRayArchAndDesignSkin(), new MaterialOptionsMentalRayArchAndDesign(), new MaterialOptionsVRayMaterial(), new MaterialOptionsStandardMaterial() };

        protected IMaterialCreator materialCreator = new MaterialOptionsMentalRayArchAndDesign();
        public IMaterialCreator MaterialCreator
        {
            set
            {
                if (value is IMaterialCreator)
                {
                    materialCreator = value;
                }
                else
                {
                    string message = "MaterialOptions must be valid object implementing CreateMaterial(MaterialWrapper)";
                    Log.Add(message, LogLevel.Error);
                    throw new ArgumentException(message);
                }
            }
        }  

        protected IEnumerable<MaterialWrapper> GetMaterials(MyMesh myMesh)
        {
            foreach (var myMat in myMesh.Materials)
            {
                yield return new MaterialWrapper(myMat, myMesh);
            }
        }

        protected IMultiMtl CreateMultiMaterial(IList<MaterialWrapper> Materials)
        {
            int NumberOfMaterialSlots = Materials.Max(Material => Material.MaterialIndex);

            IMultiMtl maxMaterial = gi.NewDefaultMultiMtl;
            maxMaterial.SetNumSubMtls(NumberOfMaterialSlots);

            foreach (var myMat in Materials)
            {
                maxMaterial.SetSubMtlAndName(myMat.MaterialIndex, materialCreator.CreateMaterial(myMat), ref myMat.MaterialName);
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

    public enum DazMtlType
    {
        DazPlastic,
        DazSkin,
        Other
    }

    /// <summary>
    /// The MaterialSource class provides an interface to the material described by a set of string properties from Daz, allowing it to be used in a stable form throughout and querying it for 
    /// information such as Opacity state
    /// </summary>
    public class MaterialWrapper
    {
        public MaterialWrapper(Material material, MyMesh mesh)
        {
            this.source_material = material;
            this.source_mesh = mesh;
            this.MaterialName = source_material.MaterialName; //because we need to pass it by ref later on.
            Initialise();
        }

        public readonly MyMesh source_mesh;
        public readonly Material source_material;

        public bool IsTransparent { get { return (opacityMapAmount == 0); } }
        public bool Initialised { get; protected set; }



        public string MaterialName;
        public int MaterialIndex { get { return source_material.MaterialIndex; } }


        public bool showInViewport = true;

        public Color ambient = Color.Black;
        public string ambientMap = null;
        public float ambientMapAmount = 1.0f;

        public string bumpMap = null;
        public float bumpMapAmount = 1.0f;

        public Color diffuse = Color.FromArgb(127, 127, 127);
        public string diffuseMap = null;
        public float diffuseMapAmount = 1.0f;

        public string opacityMap = null;
        public float opacityMapAmount = 1.0f;

        public Color specular = Color.FromArgb(230, 230, 230);
        public string specularMap = null;
        public float specularLevel = 0.0f;
        public float glossiness = 0.5f;

        public float u_tiling = 1;
        public float v_tiling = 1;

        public DazMtlType type = DazMtlType.Other;

        public void Initialise()
        {
            ambient = Defaults.AmbientGammaCorrection ? ConvertColour(source_material.GetColorSafe("Ambient Color", ambient)) : source_material.GetColorSafe("Ambient Color", ambient);
            ambientMap = source_material.GetString("Ambient Color Map");
            ambientMapAmount = source_material.GetFloatSafe("Ambient Strength", ambientMapAmount);
            bumpMap = source_material.GetString("Bump Strength Map");
            diffuseMap = source_material.GetString("Color Map");
            diffuse = ConvertColour(source_material.GetColorSafe("Diffuse Color", diffuse));
            diffuseMapAmount = source_material.GetFloatSafe("Diffuse Strength", diffuseMapAmount);
            opacityMap = source_material.GetString("Opacity Map");
            opacityMapAmount = source_material.GetFloatSafe("Opacity Strength", opacityMapAmount);
            specular = ConvertColour(source_material.GetColorSafe("Specular Color", specular));
            specularMap = source_material.GetString("Specular Color Map");
            specularLevel = source_material.GetFloatSafe("Specular Strength", specularLevel);
            glossiness = source_material.GetFloatSafe("Glossiness", glossiness);
            u_tiling = source_material.GetFloatSafe("Horizontal Tiles", u_tiling);
            v_tiling = source_material.GetFloatSafe("Vertical Tiles", v_tiling);
            bumpMapAmount = ((source_material.GetFloatSafe(new string[] { "Positive Bump", "Bump Maximum" }, 0.1f) - source_material.GetFloatSafe(new string[] { "Negative Bump", "Bump Minimum" }, -0.1f)) * source_material.GetFloatSafe("Bump Strength", 1.0f));

            type = FindType();

            Initialised = true;
        }

        #region Material Type Identification

        protected DazMtlType FindType()
        {
            switch (source_material.MaterialType)
            {
                case "DzDefaultMaterial":
                    switch (source_material.GetString("Lighting Model"))
                    {
                        case "Plastic":
                            return DazMtlType.DazPlastic;
                        case "Skin":
                            return DazMtlType.DazSkin;
                    }
                    break;
            }
            return DazMtlType.Other;
        }

        #endregion

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
}
