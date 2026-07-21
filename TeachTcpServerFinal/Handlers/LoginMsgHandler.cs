using System;
using TeachTcpServerExercises2;

namespace GameLogin
{
    public class LoginMsgHandler : BaseHandler
    {
        public override void MsgHandle()
        {
            LoginMsg msg = message as LoginMsg;
            LoginResultMsg result = AccountManager.Login(msg.account, msg.password);
            Console.WriteLine("客户端{0} 登录 账号:{1} 结果:{2} {3}",
                client.clientID, msg.account, result.result, result.info);
            _ = client.SendAsync(result);
        }
    }
}
