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
using System.IO.Pipes;


namespace MaxManagedBridge
{
    public class SceneClient
    {
        protected const string sharedMemoryName = "Local\\DazMaxBridgeSharedMemory";

        public bool Connect()
        {
            if (namedPipe == null || !namedPipe.IsConnected)
            {
                namedPipe = new NamedPipeClientStream("DazMaxBridgePipe");
                namedPipe.Connect();
                namedPipeReader = new StreamReader(namedPipe);
                namedPipeWriter = new StreamWriter(namedPipe);
            }
            return true;
        }

        public bool ConnectMemory()
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
        protected NamedPipeClientStream namedPipe;
        protected StreamReader namedPipeReader;
        protected StreamWriter namedPipeWriter;

        public MyScene GetSceneFromMemory()
        {
            MemoryMappedViewStream accessor = sharedMemory.CreateViewStream();
            accessor.Position = 0;
            MessagePackSerializer<MyScene> c = MessagePackSerializer.Create<MyScene>();
            MyScene Scene = c.Unpack(accessor);
            return Scene;
        }

        protected T GetItem<T>(string command)
        {
            namedPipeWriter.WriteLine(command);
            namedPipeWriter.Flush();

            MessagePackSerializer<T> c = MessagePackSerializer.Create<T>();
            T item = c.Unpack(namedPipeReader.BaseStream);
            return item;
        }

        public MyScene GetScene()
        {
            return GetItem<MyScene>("getScene()");
        }

        public MySceneItems GetItemList()
        {
            return GetItem<MySceneItems>("getSceneItems()");
        }

    }
}
