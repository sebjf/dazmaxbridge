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
    public class ClientManager
    {
        protected const string filter = @"\\.\pipe\";

        public ClientManager()
        {
            Instances = new List<SceneClient>();
        }

        public void FindAllInstances()
        {
            Instances.Clear();
            String[] listOfPipes = System.IO.Directory.GetFiles(filter);
            foreach (var pipe in listOfPipes)
            {
                if (pipe.Contains("DazMaxBridge"))
                {
                    var client = new SceneClient(pipe.Substring(filter.Length));
                    if (client.Reconnect())
                    {
                        Instances.Add(client);
                    }
                }
            }
        }

        public IList<SceneClient> Instances { get; protected set; }
    }

    public class SceneClient
    {
        protected NamedPipeClientStream namedPipe;
        protected StreamReader namedPipeReader;
        protected StreamWriter namedPipeWriter;

        public string DazInstanceName { get; protected set; }

        public SceneClient(string pipeName)
        {
            namedPipe = new NamedPipeClientStream(pipeName);
        }

        public bool IsConnected()
        {
            return (namedPipe != null && namedPipe.IsConnected);
        }

        public bool Reconnect()
        {
            if (IsConnected())
            {
                return true;
            }

            try
            {
                namedPipe.Connect(2000);
            }
            catch
            {
                return false;
            }

            namedPipeReader = new StreamReader(namedPipe);
            namedPipeWriter = new StreamWriter(namedPipe);

            DazInstanceName = GetItem<string>("getInstanceName()");

            return true;
        }

        protected T GetItem<T>(string command)
        {
            return GetItem<T>(new string[] { command });
        }

        protected T GetItem<T>(IEnumerable<string> commands)
        {
            if(!Reconnect())
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
            Log.Add("[m] (GetScene()) GetScene called for " + items.Count + " items.");

            items.Insert(0, "getSceneItems()");
            return GetItem<MyScene>(items);
        }

        public MySceneInformation GetSceneInformation()
        {
            return GetItem<MySceneInformation>("getMySceneInformation()");
        }

    }
}
