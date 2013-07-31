using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxManagedBridge;
using Autodesk.Max;
using Autodesk.Max.Plugins;

namespace MaxManagedBridge
{
    public partial class MaxBridgeImporterPlugin
    {
        internal IGlobal global;
        internal Descriptor descriptor;

        public MaxBridgeImporterPlugin(IGlobal global, Descriptor descriptor)
        {
            this.global = global;
            this.descriptor = descriptor;
        }
        /// <summary>
        /// Provides information about our plugin without having to create an instance of it
        /// </summary>
        public class Descriptor : ClassDesc2
        {
            IGlobal global;
            internal static IClass_ID classID;

            public Descriptor(IGlobal global)
            {
                this.global = global;

                // The two numbers used for class id have to be unique/random
                classID = global.Class_ID.Create(118920184, 157352180);
            }

            public override string Category
            {
                get { return "Daz Studio Assets"; }
            }

            public override IClass_ID ClassID
            {
                get { return classID; }
            }

            public override string ClassName
            {
                get { return "MaxBridgeImporterPlugin"; }
            }

            public override object Create(bool loading)
            {
                return new MaxBridgeImporterPlugin(global, this);
            }

            public override bool IsPublic
            {
                // true to make our plugin visible in 3dsmax interface
                get { return true; }
            }

            public override SClass_ID SuperClassID
            {
                get { return SClass_ID.SceneImport; }
            }
        }
    }
}
