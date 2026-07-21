using System;
using TeachTcpServerExercises2;

namespace GameLogin
{
    public class RegisterMsgHandler : BaseHandler
    {
        public override void MsgHandle()
        {
            RegisterMsg msg = message as RegisterMsg;
            RegisterResultMsg result = AccountManager.Register(msg.account, msg.password);
            Console.WriteLine("客户端{0} 注册 账号:{1} 结果:{2} {3}",
                client.clientID, msg.account, result.result, result.info);
            _ = client.SendAsync(result);
        }
    }
}
