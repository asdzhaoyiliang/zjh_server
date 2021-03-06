using System.Collections.Generic;

namespace MyServer
{
    /// <summary>
    /// 客户端对象连接池
    /// </summary>
    public class ClientPeerPool
    {
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int maxCount)
        {
            clientPeerQueue = new Queue<ClientPeer>(maxCount);
        }

        public void EnQueue(ClientPeer client)
        {
            clientPeerQueue.Enqueue(client);
        }

        public ClientPeer DeQueue()
        {
            return clientPeerQueue.Dequeue();
        }
    }
}