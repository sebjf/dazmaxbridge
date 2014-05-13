using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxManagedBridge;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace MaxManagedBridge
{
    /// <summary>
    /// Provides information about our plugin without having to create an instance of it
    /// </summary>
    public class MaxBridgeUtilityDescriptor : ClassDesc2
    {
        protected static IClass_ID classID;

        public MaxBridgeUtilityDescriptor(IGlobal global)
        {
            // The two numbers used for class id have to be unique/random
            classID = global.Class_ID.Create(118920184, 157352180);
        }

        public override string Category
        {
            get { return "Bridge Utility"; }
        }

        public override IClass_ID ClassID
        {
            get { return classID; }
        }

        public override string ClassName
        {
            get { return "Daz Bridge Utility"; }
        }

        /* Create may be called any number of times, such as when the utilities panel goes in and out of focus, so we use a
         * singleton to avoid multiple instances of the form. (this is the correct way to do it according to Autodesk docs.) 
         * For convenience, we use the Singleton maintained by MXSInterface, which allows the plugin to be started from here
         * or MaxScript and for the same instance always to be shared. */
        public override object Create(bool loading)
        {
            MXSInterface.Singleton.ShowForm();
            return MXSInterface.Singleton;
        }

        public override bool IsPublic
        {
            // true to make our plugin visible in 3dsmax interface
            get { return true; }
        }

        public override SClass_ID SuperClassID
        {
            get { return SClass_ID.Utility; }
        }
    }
}
