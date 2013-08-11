using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Max;

namespace MaxManagedBridge
{
    public partial class MaxPlugin
    {
        public string PrintName(System.Int64 handle)
        {
            var node = Convert(handle);
            return (node.NodeName + " of type " + node.ClassName);
        }

        public string PrintObject(object obj)
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

    }
}
