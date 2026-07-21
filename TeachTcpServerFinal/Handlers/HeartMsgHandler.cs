using System;
using TeachTcpServerExercises2;

namespace GameSystem
{
    public class HeartMsgHandler : BaseHandler
    {
        public override void MsgHandle()
        {
            HeartMsg msg = message as HeartMsg;
            client.RecordHeartbeat();
            Console.WriteLine("收到客户端{0}的心跳消息", client.clientID);
        }
    }
}
