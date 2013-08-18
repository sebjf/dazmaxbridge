using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Drawing;

namespace MaxManagedBridge
{
    /*
     * MaxScript contains alot of functionality that is unavailable or would have to be reimplemented in a plugin such as this if it were native only.
     * While we have all the information to do things such as set aribtrary properties of objects, we have to write alot of glue logic, most of which
     * has already been done.
     * 
     * Luckily there are a number of ways to interact with MaxScript:
     * 
     *  * MaxScript can use reflection to access the native properties of all .NET types in a tree once a member of it has been created in a MaxScript with 'dotNetObject'
     *  * MaxScript & .NET 'move' objects between contexts by passing Integer handles and getting the 'native' version of an object in each
     *  * .NET can execute MaxScript using SDK APIs
     *  * .NET can execute MaxScript using events which MaxScript can hook. This method allows passing of objects directly to MaxScript.
     *  
     *  To execute multiple commands in one line, they must be terminated with a semi-colon, like this:
     *  (  stdMaterial = StandardMaterial name:("3_SkinFoot"); stdMaterial.twoSided = true;stdMaterial.showInViewport = true;stdMaterial.adTextureLock = false;stdMaterial.glossiness = (0.1 as float * 100); getHandleByAnim stdMaterial;)
     *  But beware, if one command fails so will the whole line, so it is better, generally, to execute them one by one.
     */

    public partial class MaxPlugin
    {
        public string PrintHandleName(System.Int64 handle)
        {
            var node = Convert(handle);
            return (node.NodeName + " of type " + node.ClassName);
        }

        public string PrintObjectType(object obj)
        {
            return obj.GetType().GetType().Name;
        }

        public IAnimatable Convert(System.Int64 handle)
        {
            IAnimatable anim = Autodesk.Max.GlobalInterface.Instance.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }

        public System.Int64 Convert(IAnimatable obj)
        {
            return (System.Int64)Autodesk.Max.GlobalInterface.Instance.Animatable.GetHandleByAnim(obj);
        }

        public string ExecuteMaxScript(string command)
        {
            return ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(command);
        }

    }

    public class MaxScriptStandardMaterial
    {
        public string name = "standardMaxScriptMaterial";

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

        public bool twoSided = true;
        public bool showInViewport = true;
        public bool adTextureLock = true;
        public bool adLock = true;
        public bool dsLock = true;

        public Color ambient = Color.Black;
        public string ambientMap = null;
        public float ambientMapAmount = 100.0f;

        public string bumpMap = null;
        public float bumpMapAmount = 100.0f;

        public Color diffuse = Color.FromArgb(127, 127, 127);
        public string diffuseMap = null;
        public float diffuseMapAmount = 100.0f;

        public string opacityMap = null;
        public float opacityMapAmount = 100.0f;

        public Color specular = Color.FromArgb(230, 230, 230);
        public string specularMap = null;
        public float specularLevel = 0.0f;
        public float glossiness = 10.0f;

        public float u_tiling = 1;
        public float v_tiling = 1;

        public string MakeScript()
        {
            List<String> Commands = new List<string>();

            Commands.Add(string.Format("stdmaterial = StandardMaterial name:(\"{0}\")", name));

            Commands.Add(string.Format("stdmaterial.twoSided = {0}", twoSided));
            Commands.Add(string.Format("stdmaterial.showInViewport = {0}", showInViewport));
            Commands.Add(string.Format("stdmaterial.adTextureLock = {0}", adTextureLock));
            Commands.Add(string.Format("stdmaterial.adLock = {0}", adLock));
            Commands.Add(string.Format("stdmaterial.dsLock = {0}", dsLock));

            if (ambientMap != null)
            {
                Commands.Add(string.Format("stdmaterial.ambientMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", ambientMap, ambient.R, ambient.G, ambient.B));
                Commands.Add(string.Format("stdmaterial.ambientMapAmount = {0}", ambientMapAmount));
                Commands.Add(string.Format("stdmaterial.ambientMap.map1.coords.u_tiling = {0}; stdmaterial.ambientMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));
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
            }

            if (diffuseMap != null)
            {
                Commands.Add(string.Format("stdmaterial.diffuseMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", diffuseMap, diffuse.R, diffuse.G, diffuse.B));
                Commands.Add(string.Format("stdmaterial.diffuseMapAmount = {0}", diffuseMapAmount));
                Commands.Add(string.Format("stdmaterial.diffuseMap.map1.coords.u_tiling = {0}; stdmaterial.diffuseMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.diffuse = color {0} {1} {2}", diffuse.R, diffuse.G, diffuse.B));
            }

            if (opacityMap != null)
            {
                Commands.Add(string.Format("stdmaterial.opacityMap = (bitmapTexture filename:\"{0}\")", opacityMap));
                Commands.Add(string.Format("stdmaterial.opacityMapAmount = {0}", opacityMapAmount));
                Commands.Add(string.Format("stdmaterial.opacityMap.coords.u_tiling = {0}; stdmaterial.opacityMap.coords.v_tiling = {1};", u_tiling, v_tiling));
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.opacity = {0}", opacityMapAmount));
            }

            if (specularMap != null)
            {
                Commands.Add(string.Format("stdmaterial.specularMap = RGB_Multiply map1:(bitmapTexture filename:\"{0}\") color2:(color {1} {2} {3})", specularMap, specular.R, specular.G, specular.B));
                Commands.Add(string.Format("stdmaterial.specularMap.map1.coords.u_tiling = {0}; stdmaterial.specularMap.map1.coords.v_tiling = {1};", u_tiling, v_tiling));
            }
            else
            {
                Commands.Add(string.Format("stdmaterial.specular = (color {0} {1} {2})", specular.R, specular.G, specular.B));
            }

            Commands.Add(string.Format("stdmaterial.specularLevel = {0}", specularLevel));
            Commands.Add(string.Format("stdmaterial.glossiness = {0}", glossiness));

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
