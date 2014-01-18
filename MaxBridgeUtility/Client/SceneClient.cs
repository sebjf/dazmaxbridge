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
using System.Runtime.InteropServices;


namespace MaxManagedBridge
{
    unsafe public class SharedMemory
    {
        protected MemoryMappedFile memoryMappedFile;
        protected MemoryMappedViewAccessor memoryMappedAccessor;
        protected string name;
        protected byte* ptr;
        public int size;

        public SharedMemory()
        {
            ptr = (byte*)0;
        }

        public byte* Open(string name)
        {
            if (this.name == name)
            {
                return ptr;
            }

            if (memoryMappedAccessor != null)
            {
                memoryMappedAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                memoryMappedAccessor = null;
            }

            memoryMappedFile = MemoryMappedFile.OpenExisting(name);
            memoryMappedAccessor = memoryMappedFile.CreateViewAccessor();
            memoryMappedAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);

            size = (int)memoryMappedAccessor.SafeMemoryMappedViewHandle.ByteLength;

            return ptr;
        }
    }

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

        protected SharedMemory sharedMemory = new SharedMemory();

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

        unsafe protected T GetItem<T>(IEnumerable<string> commands)
        {
            if(!Reconnect())
            {
                return default(T);
            }

            Log.Add("[m] (GetItem<T>()) Sending request...");

            foreach (var command in commands)
            {
                namedPipeWriter.WriteLine(command);
            }
            namedPipeWriter.Flush();

            MessagePackSerializer<T> c = MessagePackSerializer.Create<T>();

            if (commands.FirstOrDefault() == "getSceneItems()")
            {
                string name = namedPipeReader.ReadLine();
                byte* ptr = sharedMemory.Open(name);
                byte[] array = new byte[sharedMemory.size];
                Marshal.Copy(new IntPtr(ptr), array, 0, sharedMemory.size);
                return c.UnpackSingleObject(array);
            }
            else
            {
                T item = c.Unpack(namedPipeReader.BaseStream);
                return item;
            }

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
