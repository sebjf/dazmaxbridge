using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MaxManagedBridge
{
    /*
     * MaxScript contains alot of functionality that is unavailable (such as FP - Function Publishing - Interfaces) or would have to be reimplemented with glue logic, such as
     * with creating a BitmapTex from an image file.
     * 
     * Alternatively, there are a number of ways to interact with MaxScript:
     * 
     *  * MaxScript can use reflection to access the native properties of all .NET types in a graph once a member of it (the graph) has been created in a MaxScript with 'dotNetObject'
     *  * MaxScript & .NET 'move' objects between contexts by passing Integer handles and getting the 'native' version of an object in each
     *  * .NET can execute MaxScript using SDK APIs
     *  * .NET can execute MaxScript using events which MaxScript can hook. This method allows passing of objects directly to MaxScript.
     *  
     *  To execute multiple commands in one line, they must be terminated with a semi-colon, like this:
     *  (  stdMaterial = StandardMaterial name:("3_SkinFoot"); stdMaterial.twoSided = true;stdMaterial.showInViewport = true;stdMaterial.adTextureLock = false;stdMaterial.glossiness = (0.1 as float * 100); getHandleByAnim stdMaterial;)
     *  But beware, if one command fails so will the whole line.
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
            IAnimatable anim = gi.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }

        public System.Int64 Convert(IAnimatable obj)
        {
            return (System.Int64)gi.Animatable.GetHandleByAnim(obj);
        }

        public string ExecuteMaxScript(string command)
        {
            return ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(command);
        }

    }

    /*
        Welcome to MAXScript.

        c = dotNetObject "MaxManagedBridge.MXSInterface"
        dotNetObject:MaxManagedBridge.MXSInterface

        m = c.GetPlugin()
        dotNetObject:MaxManagedBridge.MaxPlugin

        showproperties m
          .MaterialCreator : <MaxManagedBridge.IMaterialCreator>, write-only
          .RebuildMaterials : <System.Boolean>
          .RemoveTransparentFaces : <System.Boolean>
          .SceneNodes : <System.Collections.Generic.IEnumerable`1[Autodesk.Max.IINode]>, read-only
          .Templates : <MaxManagedBridge.MaterialLibraryView>
          .AvailableMaterialCreators : <MaxManagedBridge.IMaterialCreator[]>, read-only
          .DazClientManager : <MaxManagedBridge.ClientManager>, read-only
          .ProgressCallback : <MaxManagedBridge.MaxPlugin+ProgressUpdateHandler>
        true
    */

    public class MXSInterface
    {
        public static MaxPlugin plugin;
        public MaxPlugin GetPlugin()
        {
            return plugin;
        }

        public void PutPlugin(object o)
        {
            System.Console.Write(o);
        }
    }

    public abstract class MaxSDKTool
    {
        public IInterface intrface { get { return GlobalInterface.Instance.COREInterface; } }
        public IGlobal gi { get { return GlobalInterface.Instance; } }
    }

    public abstract class MaxScriptMaterialGenerator : MaxSDKTool
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
                    "Exception: Could not create mtl.",
                    MessageBoxButtons.OK);
            }

            return null;
        }

        protected IAnimatable Convert(System.Int64 handle)
        {
            IAnimatable anim = gi.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }

        protected UIntPtr Convert(IAnimatable anim)
        {
            return gi.Animatable.GetHandleByAnim(anim);
        }
    }

}
