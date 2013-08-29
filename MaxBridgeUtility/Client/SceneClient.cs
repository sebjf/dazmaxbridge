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
        protected MemoryMappedFile sharedMemory;
        protected NamedPipeClientStream namedPipe;
        protected StreamReader namedPipeReader;
        protected StreamWriter namedPipeWriter;

        protected bool Connect()
        {
            if (namedPipe == null || !namedPipe.IsConnected)
            {
                namedPipe = new NamedPipeClientStream("DazMaxBridgePipe");
                namedPipe.Connect(2000);

                if (!namedPipe.IsConnected)
                {
                    MessageBox.Show("Unable to find Daz! Is it running?");
                }

                namedPipeReader = new StreamReader(namedPipe);
                namedPipeWriter = new StreamWriter(namedPipe);
            }
            return true;
        }

        protected T GetItem<T>(string command)
        {
            if (!Connect())
            {
                return default(T);
            }

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
