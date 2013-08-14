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

        //protected IMultiMtl MakeMultiMaterial(MaxMesh mesh)
        //{
        //    IMultiMtl myMaterial = global.NewDefaultMultiMtl;

        //    myMaterial.SetNumSubMtls(mesh.Materials.Keys.ToArray().Max() + 1);

        //    foreach (int i in mesh.Materials.Keys.ToArray())
        //    {
        //        myMaterial.SetSubMtl(i, MakeStandardMaterial(mesh.Materials[i]));
        //    }

        //    return myMaterial;
        //}

        //protected IMtl MakeStandardMaterial(Material material)
        //{
        //    IStdMat2 myMaterial = global.NewDefaultStdMat;

        //    myMaterial.Name = material.MaterialName;
        //    myMaterial.TwoSided = true;
        //    myMaterial.LockAmbDiffTex(false);



        //    return myMaterial;
        //}

        protected void UpdateMaterial(IStdMat2 mat, string property, string value)
        {
            switch (property)
            {
                /*
                case    "Ambient Color"     : mat.SetAmbient(toColor(value), 0); break;
                case	"Ambient Color Map"	: myMaterial.ambientMap					= toMap myValue
                case	"Ambient Strength"	: myMaterial.ambientMapAmount 		= toPercent myValue
                case	"Bump Strength Map"	: myMaterial.bumpMap 						= toMap myValue
                case	"Color Map"			: (getDiffuseMap myMaterial).map1 	= toMap myValue
                case	"Diffuse Color"		: (getDiffuseMap myMaterial).color2 	= toColour myValue
                case	"Diffuse Strength"	: myMaterial.diffuseMapAmount 			= toPercent myValue
                case	"Opacity Map"		: myMaterial.opacityMap 					= toMap myValue
                case	"Opacity Strength"	: myMaterial.opacityMapAmount			= toPercent myValue
                case	"Specular Color"	: myMaterial.specular 						= toColour myValue
                case	"Specular Color Map": myMaterial.specularMap					= toMap myValue
                case	"Specular Strength"	: myMaterial.specularLevel 				= toPercent myValue
                case	"Glossiness"	    :
                */
            }
        }

        
        //IColor toColor(string value)
        //{
        //    var components = value.Split(' ');
        //    return global.Color.Create(float.Parse(components[1]), float.Parse(components[2]), float.Parse(components[3]));
        //}
        
    }
}
