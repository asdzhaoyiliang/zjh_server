namespace MyServer
{
    public interface IApplication
    {
        /**
         * 断开连接
         */
        void Disconnect(ClientPeer client);
        /**
         * 接收数据
         */
        void Receive(ClientPeer client, NetMsg msg);
    }
}