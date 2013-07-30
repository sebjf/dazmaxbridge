using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MaxBridgeLib;
using Autodesk.Max;
using Autodesk.Max.Plugins;


namespace MaxBridgePlugin
{
    public partial class MaxBridgePlugin : Autodesk.Max.Plugins.SceneImport
    {
        public override string AuthorName
        {
            get { throw new NotImplementedException(); }
        }

        public override string CopyrightMessage
        {
            get { throw new NotImplementedException(); }
        }

        public override int DoImport(string name, IImpInterface ii, IInterface i, bool suppressPrompts)
        {
            throw new NotImplementedException();
        }

        public override string Ext(int n)
        {
            throw new NotImplementedException();
        }

        public override int ExtCount
        {
            get { throw new NotImplementedException(); }
        }

        public override string LongDesc
        {
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
        }

        public override void ShowAbout(IntPtr hWnd)
        {
            throw new NotImplementedException();
        }

        public override uint Version
        {
            get { throw new NotImplementedException(); }
        }

        public override int ZoomExtents
        {
            get { throw new NotImplementedException(); }
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
