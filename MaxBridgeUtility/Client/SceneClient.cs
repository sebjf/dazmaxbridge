using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Windows.Forms;
using MsgPack.Serialization;
using MsgPack;


namespace MaxManagedBridge
{
    public class SceneClient
    {
        protected const string sharedMemoryName = "Local\\DazMaxBridgeSharedMemory";

        public bool Connect()
        {
            if (sharedMemory != null)
            {
                return true;
            }

            try
            {
                sharedMemory = MemoryMappedFile.OpenExisting(sharedMemoryName, MemoryMappedFileRights.ReadWrite, HandleInheritability.None);
            }
            catch (FileNotFoundException nfe)
            {
                MessageBox.Show("Unable to find a Daz instance to connect to!");
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show("Unknown error has occured.");
                return false;
            }  

            return true;
        }

        protected MemoryMappedFile sharedMemory;

        public MyScene GetScene()
        {
            MemoryMappedViewStream accessor = sharedMemory.CreateViewStream();
            accessor.Position = 0;
            MessagePackSerializer<MyScene> c = MessagePackSerializer.Create<MyScene>();
            MyScene Scene = c.Unpack(accessor);
            return Scene;
        }
    }
}
