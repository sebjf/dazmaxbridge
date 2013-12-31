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
            Plugin = new MaxPlugin(Global);
            GUI = new UtilityMainForm(this);
            GUI.ShowModeless();
        }

        protected IInterface Interface;
        protected IIUtil UtilityInterface;
        protected IGlobal Global;
        protected ILogSys LogSys;

        protected UtilityMainForm GUI;
        public    MaxPlugin Plugin; 

        public override void BeginEditParams(IInterface ip, IIUtil iu)
        {
            Interface = ip;
            UtilityInterface = iu;
            LogSys = ip.Log;
            Log.logger = LogSys;
            Log.EnableLog = true;
//          ip.PushPrompt();
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
