using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxManagedBridge;
using Autodesk.Max;
using Autodesk.Max.Plugins;


namespace MaxManagedBridge
{
    public partial class MaxImporter : Autodesk.Max.Plugins.SceneImport
    {
        public override string AuthorName
        {
            get { return "Daz Max Bridge Author"; }
        }

        public override string CopyrightMessage
        {
            get { return "Daz Max Bridge Copyright Message"; }
        }

        IImpInterface importer;
        IInterface ginteface;

        public override int DoImport(string name, IImpInterface ii, IInterface i, bool suppressPrompts)
        {
            importer = ii;
            ginteface = i;

            MaxPlugin myBridge = new MaxPlugin();

            myBridge.LoadFromFile(name);

            ImportUsingHeadlessPlugin(myBridge);
           
            return 1;
        }

        public override string Ext(int n)
        {
            return "characterkit";
        }

        public override int ExtCount
        {
            get { return 1; }
        }

        public override string LongDesc
        {
            get { return "Importer for DazMaxBridge bytestreams."; }
        }

        public override string OtherMessage1()
        {
            throw new NotImplementedException();
        }

        public override string OtherMessage2()
        {
            throw new NotImplementedException();
        }

        public override string ShortDesc
        {
            get { return "DazMaxBridge Bytestream"; }
        }

        public override void ShowAbout(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        public override uint Version
        {
            get { return 0; }
        }

        public override int ZoomExtents
        {
            get { return 0; }
        }

        public override void Dispose()
        {
        }

    }
}
