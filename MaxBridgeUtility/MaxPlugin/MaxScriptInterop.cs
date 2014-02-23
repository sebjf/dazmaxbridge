using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;
using System.Drawing;

namespace MaxManagedBridge
{
    /*
     * MaxScript contains alot of functionality that is unavailable or would have to be reimplemented in a plugin like this one.
     * While we have all the information to do things such as set aribtrary properties of objects, we have to write alot of glue logic, which Autodesk as already done and made available in MaxScript.
     * 
     * Luckily there are a number of ways to interact with MaxScript:
     * 
     *  * MaxScript can use reflection to access the native properties of all .NET types in a graph once a member of it has been created in a MaxScript with 'dotNetObject'
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
            IAnimatable anim = globalInterface.Animatable.GetAnimByHandle((UIntPtr)handle);
            return anim;
        }

        public System.Int64 Convert(IAnimatable obj)
        {
            return (System.Int64)globalInterface.Animatable.GetHandleByAnim(obj);
        }

        public string ExecuteMaxScript(string command)
        {
            return ManagedServices.MaxscriptSDK.ExecuteStringMaxscriptQuery(command);
        }

    }

}
