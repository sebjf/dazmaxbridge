using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MaxBridgeLib;

namespace MaxBridgeLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MaxBridge bridge = new MaxBridge();
            bridge.LoadFromFile(@"E:\Daz3D\Scripting\f.characterkit");
            int[] f = bridge.GetTriangulatedFaces();
        }


    }
}
