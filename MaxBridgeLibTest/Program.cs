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
            MaxBridgePlugin p = new MaxBridgePlugin();
            p.LoadFromFile(@"E:\Daz3D\Scripting\Scratch.characterkit");

        }


    }
}
