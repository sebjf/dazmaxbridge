using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxManagedBridge;
using Autodesk.Max;
using Autodesk.Max.Plugins;


namespace MaxManagedBridge
{
    public static class AssemblyFunctions
    {
        public static void AssemblyMain()
        {
            var g = Autodesk.Max.GlobalInterface.Instance;
            var i = g.COREInterface13;
            i.AddClass(new MaxBridgeUtilityDescriptor(g));
        }

        public static void AssemblyShutdown()
        {
        }
    }

    public partial class MaxBridgeUtility : Autodesk.Max.Plugins.UtilityObj
    {
        public MaxBridgeUtility()
        {
            Global = Autodesk.Max.GlobalInterface.Instance;
            Log.logger = Global.COREInterface.Log;
            Log.EnableLog = true;
            Log.Add("[m] Starting DazMaxBridge.");

            Plugin = new MaxPlugin(Global);

            Log.Add("[m] Starting UI.");

            GUI = new UtilityMainForm(this);
            GUI.ShowModeless();
        }

        protected IInterface Interface;
        protected IIUtil UtilityInterface;
        protected IGlobal Global;

        protected UtilityMainForm GUI;
        public    MaxPlugin Plugin; 

        public override void BeginEditParams(IInterface ip, IIUtil iu)
        {
            Interface = ip;
            UtilityInterface = iu;
        }

        public override void EndEditParams(IInterface ip, IIUtil iu)
        {
//            ip.PopPrompt();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
