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

            m  = p.CreateMaterial(p.Scene.Items[0].Materials[0]);

            //MaxBridgeUtility u = new MaxBridgeUtility();

            

            //p.LoadFromDaz();

        }


    }
}
