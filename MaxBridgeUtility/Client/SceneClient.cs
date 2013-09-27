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

        protected bool EnsureConnectionToDaz()
        {
            if (namedPipe == null || !namedPipe.IsConnected)
            {
                namedPipe = new NamedPipeClientStream("DazMaxBridgePipe");
                try
                {
                    namedPipe.Connect(2000);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to find Daz! Is it running?");
                    return false;
                }

                namedPipeReader = new StreamReader(namedPipe);
                namedPipeWriter = new StreamWriter(namedPipe);
            }

            return true;
        }

        protected T GetItem<T>(string command)
        {
            return GetItem<T>(new string[] { command });
        }

        protected T GetItem<T>(IEnumerable<string> commands)
        {
            if (!EnsureConnectionToDaz())
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
