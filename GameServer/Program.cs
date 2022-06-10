using System;
using MyServer;

namespace GameServer
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ServerPeer server = new ServerPeer();
            server.SetApplication(new NetMsgCenter());
            server.StartServer("127.0.0.1",6666,100);
            Database.DatabaseManager.StartConnect();
            Console.ReadKey();
        }
    }
}