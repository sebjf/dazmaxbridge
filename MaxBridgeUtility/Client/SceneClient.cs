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
            Log.Add("[m] (GetItem<T>()) Checking connection...");

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

            Log.Add("[m] (GetItem<T>()) Sent...Waiting for pipe drain...");

            namedPipe.WaitForPipeDrain();

            Log.Add("[m] Fetching deserialiser...");

            /* We find that
             * MessagePackSerializer<T> c = MessagePackSerializer.Create<T>(); 
             * can cause Max to crash. Not sure why but until then we will prebuild the deserialisers
             * and reuse them, hence the following... */ 

     //       MessagePackSerializer<T> c = MessagePackSerializer.Create<T>();
            MessagePackSerializer<T> c = MessagePackSerialisers.GetUnpacker<T>();

            if (commands.FirstOrDefault() == "getSceneItems()")
            {
                Log.Add("[ma] (GetItem<T>()) Reading shared mem name from pipe...");
                string name = namedPipeReader.ReadLine();

                Log.Add("[ma] (GetItem<T>()) Opening shared memory...");
                byte* ptr = sharedMemory.Open(name);
                byte[] array = new byte[sharedMemory.size];
                Marshal.Copy(new IntPtr(ptr), array, 0, sharedMemory.size);

                Log.Add("[ma] Unpacking...");
                T item = c.UnpackSingleObject(array);

                Log.Add("[ma] (GetItem<T>()) Unpacked..returning scene update.");
                return item;

            }
            else
            {
                Log.Add("[mb] Unpacking from stream...");
                T item = c.Unpack(namedPipeReader.BaseStream);

                Log.Add("[mb] (GetItem<T>()) Unpacked..returning scene update.");
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

    class MessagePackSerialisers
    {
        /* Put the deserialiser code in a shared class, because if it breaks easily we want to run it as few times as possible */

        public static MessagePackSerializer<T> GetUnpacker<T>()
        {
            return instance.unpackers[typeof(T)] as MessagePackSerializer<T>;
        }

        protected static MessagePackSerialisers instance = new MessagePackSerialisers();

        protected MessagePackSerialisers()
        {
            Log.Add("[m] Building deserialisers...");

            CreateUnpacker<MyScene>();
            CreateUnpacker<string>();
            CreateUnpacker<MySceneInformation>();
        }

        protected void CreateUnpacker<T>()
        {
            unpackers[typeof(T)] = MessagePackSerializer.Create<T>();
        }

        protected Dictionary<Type, object> unpackers = new Dictionary<Type, object>();
    }
}
