using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using Autodesk.Max;
using System.Windows.Forms;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;

namespace MaxManagedBridge
{
    //http://docs.autodesk.com/3DSMAX/16/ENU/3ds-Max-SDK-Programmer-Guide/index.html?url=files/GUID-5243C5F6-2835-430C-8E17-617700BC6D0D.htm,topicNumber=d30e28922

    public interface IIMtlBaseView
    {
        string Name { get; }
        IMtlBase Mtl { get; }
    }

    public class IMtlBaseView : IIMtlBaseView
    {
        public string Name { get { return Mtl.Name; } }
        public IMtlBase Mtl { get; private set; }

        public IMtlBaseView(IMtlBase mtl)
        {
            Mtl = mtl;
        }
    }

    //public IMtlBase GetTemplateMaterial(MyMesh myMesh)
    //{
    //    IMtlBase template = TemplateMaterial;

    //    if (FindTemplateByMeshName)
    //    {
    //        //search all the material libraries and sample slots for a matching name
    //        foreach (var m in Templates)
    //        {
    //            if (m.Name == myMesh.Name)
    //            {
    //                return m.Mtl;
    //            }
    //        }

    //        for (int i = 0; i < 24; i++)
    //        {
    //            if (globalInterface.COREInterface.GetMtlSlot(i).Name == myMesh.Name)
    //            {
    //                return globalInterface.COREInterface.GetMtlSlot(i);
    //            }
    //        }
    //    }

    //    return template;
    //}

    public class NameMatchView : IIMtlBaseView
    {
        public string Name
        {
            get { return "Match Material by Model Name"; }
        }

        public NameMatchView(MaterialLibraryView lib)
        {
            this.library = lib;
        }

        public IMtlBase Mtl
        {
            get { throw new NotImplementedException(); } 
        }

        private MaterialLibraryView library;
    }

    public class SampleSlotView : IIMtlBaseView
    {
        public string Name
        {
            get { return string.Format("Sample Slot {0}", id); }
        }

        public IMtlBase Mtl
        {
            get { return GlobalInterface.Instance.COREInterface.GetMtlSlot(id); }
        }

        private int id;

        public SampleSlotView(int slotid)
        {
            id = slotid;
        }
    }

    /* An interface to a material library, which the user can work with in the SME */
    public class MaterialLibraryView : BindingList<IIMtlBaseView>
    {
        protected const int APP_MATLIB_DIR = 14;

        public MaterialLibraryView(string name)
        {
            context = SynchronizationContext.Current;
            filename = Path.Combine(GlobalInterface.Instance.COREInterface.GetDir(APP_MATLIB_DIR), name);
            CreateFilesystemWatch(filename);
            if (!ReloadLibrary())
            {
                MessageBox.Show("Could not open material library. See log for more details");
                Log.Add(string.Format("Failed to open material library {0}. If a populated material library is not available, default materials will be used as templates when creating advanced materials. Create and populate a library using the SME.", filename), LogLevel.Error);
            }
        }

        protected SynchronizationContext context;
        protected string filename;

        protected bool ReloadLibrary()
        {
            ClearItems();

            var lib = GlobalInterface.Instance.MtlBaseLib.Create();
            bool success = GlobalInterface.Instance.COREInterface.LoadMaterialLib(filename, lib) != 0;

            if (success)
            {
                for (int i = 0; i < lib.Count; i++)
                {
                    Add(new IMtlBaseView(lib[(IntPtr)i]));
                }
            }

            //Todo: seperate out material template wrapper functionality and actual material library wrapper functionality

            Add(new SampleSlotView(0));
            Add(new SampleSlotView(1));
            Add(new SampleSlotView(2));

            return success;
        }

        // http://stackoverflow.com/questions/721714/notification-when-a-file-changes

        protected void CreateFilesystemWatch(string filename)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Path.GetDirectoryName(filename), Path.GetFileName(filename));
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime | NotifyFilters.Size;
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.EnableRaisingEvents = true;
        }

        protected void OnChanged(object source, FileSystemEventArgs e)
        {
            //Altering the bindinglist in ReloadLibrary() will attempt to access the drop down control, so we must ensure that it is called in the original 
            //thread that created the control (found with Context.Current in the ctor), as OnChanged (this method) will be called by the thread that monitors the filesystem
            context.Send(ReloadLibraryCallback, null);
        }

        protected void ReloadLibraryCallback(object o)
        {
            ReloadLibrary();
        }
        
    }
}
