using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyServer
{
    public class ServerPeer
    {
        //服务器socket
        private Socket serverSocket;

        //计量器
        private Semaphore semaphore;

        //客户端对象连接池
        private ClientPeerPool clientPeerPool;

        //应用层
        private IApplication application;

        /**
         * 设置应用层
         */
        public void SetApplication(IApplication application)
        {
            this.application = application;
        }

        /**
         * 开启服务器
         */
        public void StartServer(string ip, int port, int maxClient)
        {
            try
            {
                clientPeerPool = new ClientPeerPool(maxClient);
                semaphore = new Semaphore(maxClient, maxClient);
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //填满客户端对象连接池
                for (int i = 0; i < maxClient; i++)
                {
                    ClientPeer temp = new ClientPeer();
                    temp.receiveCompleted = ReceiveProcessCompleted;
                    temp.ReceiveArgs.Completed += ReceiveArgs_Completed;
                    clientPeerPool.Enqueue(temp);
                }

                //绑定到进程
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                //最大的监听数
                serverSocket.Listen(maxClient);
                Console.WriteLine("服务器启动成功");
                StartAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        #region 接收客户端的连接请求

        /**
         * 接收客户端的连接
         */
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }

            //如果result为true，正在接收，成功后触发Completed事件
            //如果result为false，接收成功
            bool result = serverSocket.AcceptAsync(e);
            if (result == false)
            {
                ProcessAccept(e);
            }

        }

        /**
         * 异步接收客户端的连接完成后触发
         */
        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        /**
         * 处理连接请求
         */
        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            semaphore.WaitOne();
            ClientPeer client = clientPeerPool.Dequeue();
            client.clientSocket = e.AcceptSocket;
            Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端连接成功");
            //接收消息
            StartReceive(client);
            e.AcceptSocket = null;
            StartAccept(e);
        }

        #endregion

        #region 接收数据

        /**
         * 开始接收数据
         */
        private void StartReceive(ClientPeer client)
        {
            try
            {
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                if (result == false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        /**
         * 异步接收数据完成后调用
         */
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        /**
         * 处理数据的接收
         */
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            ClientPeer client = e.UserToken as ClientPeer;
            //判断数据是否接收成功
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];
                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);

                //让ClientPeer自身处理数据
                client.ProcessReceive(packet);
                StartReceive(client);
            }
            //断开连接
            else
            {
                //没有字节数代表断开连接了
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //客户端主动断开连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnect(client, "客户端主动断开连接");
                    }
                    //网络异常被动断开连接
                    else
                    {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }

        /**
         * 一条消息处理完成后的回调
         */
        private void ReceiveProcessCompleted(ClientPeer client, NetMsg msg)
        {
            //交给应用层处理这个消息
            application.Receive(client, msg);
        }

        #endregion

        #region 断开连接

        /**
         * 客户端断开连接
         */
        private void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空，无法断开连接");
                }

                Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端断开连接，原因：" + reason);
                application.Disconnect(client);
                //让客户端处理断开连接
                client.Disconnect();

                clientPeerPool.Enqueue(client);
                semaphore.Release();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        #endregion

    }
}