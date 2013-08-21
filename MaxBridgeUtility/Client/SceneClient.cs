using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using MsgPack.Serialization;
using MsgPack;
using System.IO.MemoryMappedFiles;

namespace MaxManagedBridge
{
    public class SceneClient
    {
        public void Connect()
        {
            Socket = new TcpClient("localhost", 12121);
            Socket.ReceiveBufferSize = 100000000; //nasty nasty

            Stream s = Socket.GetStream();
            Reader = new StreamReader(s);
            Writer = new StreamWriter(s);
            Writer.AutoFlush = true;
        }

        private TcpClient Socket;

        private StreamReader Reader;
        private StreamWriter Writer;

        public MyScene GetScene()
        {
            MemoryMappedFile f; ///MAX SUPPORTS .NET 4 yayayy
            

            Writer.WriteLine("getScene");
            MessagePackSerializer<MyScene> c = MessagePackSerializer.Create<MyScene>();
            MyScene Scene = c.Unpack(Reader.BaseStream);
            return Scene;
        }
    }
}
