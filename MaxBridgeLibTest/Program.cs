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
            int vert_count = bridge.GetNumVertices();
            float[] verts  = bridge.GetVertices();
            int facet_count = bridge.GetNumFaces();
            int[] facets = bridge.GetFaces();
            string[] matprops = bridge.GetMaterialProperties(1);
            string[] matvals = bridge.GetMaterialValues(1);

            int[] f = bridge.GetTriangulatedFaces();

            
            FileStream fs = new FileStream(@"E:\materials.txt",FileMode.OpenOrCreate,FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            for(int i = 0; i < matprops.Length; i++)
            {
                writer.WriteLine( matprops[i] + " : " + matvals[i]);
            }
            writer.Close();
            fs.Close();
        }


    }
}
