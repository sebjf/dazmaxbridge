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
            bridge.LoadFromFile(@"E:\Daz3D\Scripting\f1.characterkit");
            int vert_count = bridge.GetNumVertices();
            float[] verts  = bridge.GetVertices();
            int facet_count = bridge.GetNumFaces();
            int[] facets = bridge.GetFaces();

        }
    }
}
