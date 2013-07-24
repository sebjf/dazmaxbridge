using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MaxBridgeLib;

namespace MaxBridgeLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MaxBridge bridge = new MaxBridge();
            bridge.LoadFromFile(@"E:\Daz3D\Scripting\f2.characterkit");
            int vert_count = bridge.GetNumVertices();
            float[] verts  = bridge.GetVertices();
            int facet_count = bridge.GetNumFaces();
            int[] facets = bridge.GetFaces();
            string[] matprops = bridge.GetMaterialProperties(1);
            string s = bridge.GetMaterialProperty(0, "BaseOpacity");
        }
    }
}
