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
                try
                {
                    namedPipe.Connect(2000);
                }
                catch (TimeoutException e)
                {
                    MessageBox.Show("Unable to find Daz! Is it running?");
                }

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
            List<string> list = new List<string>();
            list.Add(command);
            return GetItem<T>(list);
        }

        protected T GetItem<T>(IList<string> commands)
        {
            if (!Connect())
            {
                return default(T);
            }

            foreach (var command in commands)
            {
                namedPipeWriter.WriteLine(command);
            }
            namedPipeWriter.Flush();

            MessagePackSerializer<T> c = MessagePackSerializer.Create<T>();
            T item = c.Unpack(namedPipeReader.BaseStream);
            return item;
        }

        public MyScene GetScene()
        {
            return GetItem<MyScene>(new List<string>());
        }

        public MyScene GetScene(IList<string> items)
        {
            items.Insert(0, "getSceneItems()");
            return GetItem<MyScene>(items);
        }

        public MySceneInformation GetSceneInformation()
        {
            return GetItem<MySceneInformation>("getMySceneInformation()");
        }

    }
}
