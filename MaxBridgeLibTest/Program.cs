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

            var sceneinfo = p.DazClient.GetSceneInformation();

            List<string> list = new List<string>();
            //list.Add("Genesis");

            MyScene fs = p.DazClient.GetScene(list);

        }


    }
}
