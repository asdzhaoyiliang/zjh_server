namespace MyServer
{
    /**
     * 网络消息类
     * 每次发送消息都发送这个类，接收到消息后需要转换成这个类
     */
    public class NetMsg
    {
        public int opCode;
        public int subCode;
        public object value;

        public NetMsg()
        {
            
        }
        public NetMsg(int opCode, int subCode, object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }

        public void Change(int opCode, int subCode, object value)
        {
            this.opCode = opCode;
            this.subCode = subCode;
            this.value = value;
        }
    }
}