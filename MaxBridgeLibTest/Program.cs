using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using MaxManagedBridge;

namespace MaxBridgeLibTest
{
    class Program
    {
        static void Main(string[] args)
        {
            MaxPlugin p = new MaxPlugin();
            p.LoadFromFile(@"E:\Daz3D\Scripting\Scratch.dazmaxbridge");

            //MaxBridgeUtility u = new MaxBridgeUtility();

            MaxScriptStandardMaterial m = new MaxScriptStandardMaterial();

            string s = m.MakeScript();

        }


    }
}
