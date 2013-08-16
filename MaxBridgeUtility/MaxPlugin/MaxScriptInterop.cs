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

        public static string ToMapCommand(string value)
        {
            if (value == null || value.Length <= 0)
            {
                return "";
            }
            return string.Format("(bitmapTexture filename:\"{0}\")",value);
        }

        public static string ToColourCommand(string value)
        {
            if (value == null || value.Length <= 0)
            {
                return "";
            }
            string[] components = value.Split(' ');
            return string.Format("(color ({0} as float * 255) ({1} as float * 255) ({2} as float * 255) ({3} as float * 255) )", components[1], components[2], components[3], components[0]);
        }

        public static string ToColourCommand(Color value)
        {
            return string.Format("(color (0} {1} {2} {3})", value.R, value.G, value.B, value.A);
        }

        public static string ToPercentCommand(string value)
        {
            if (value == null || value.Length <= 0)
            {
                return "";
            }
            return string.Format("({0} as float * 100)", value);

        }

    }

    public class MaxScriptObjectBuilder
    {
        public MaxScriptObjectBuilder(string name)
        {
            this.Name = name;
        }

        public string Name;

        public string this[string key]
        {
            set
            {
                if (value.Length <= 0)
                {
                    return;
                }

                Commands.Add(string.Format("{0}.{1} = {2}", Name, key, value));
            }
        }

        public List<string> Commands = new List<string>();

    }

}
