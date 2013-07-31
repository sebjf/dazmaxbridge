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
        protected void ImportUsingHeadlessPlugin(MaxBridgePlugin headlessPlugin)
        {
            foreach (MaxMesh mesh in headlessPlugin.Scene.Items)
            {
                ITriObject m = global.TriObject.Create();
                headlessPlugin.PopulateMesh(m.Mesh, mesh);
            }
        }

        
    }
}
