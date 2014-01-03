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
            MaxBridge p = new MaxBridge();

            p.DazClientManager.FindAllInstances();

           

            string s = p.DazClientManager.Instances.First().DazInstanceName;

           

            List<string> list = new List<string>();
            //list.Add("Genesis");

            MyScene fs = p.DazClientManager.Instances[0].GetScene(list);

        }


    }
}
