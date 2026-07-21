using System.Collections.Generic;
using GameLogin;

namespace TeachTcpServerExercises2
{
    // 账号管理：内存字典存储 account -> password，进程重启即清空。
    // result 约定与客户端一致：0 = 成功，非 0 = 失败，info 直接给客户端显示。
    static class AccountManager
    {
        private static readonly Dictionary<string, string> accounts = new Dictionary<string, string>();
        //字典本身在并发收消息线程中读写，需要加锁
        private static readonly object locker = new object();

        public static RegisterResultMsg Register(string account, string password)
        {
            RegisterResultMsg msg = new RegisterResultMsg();
            lock (locker)
            {
                if (accounts.ContainsKey(account))
                {
                    msg.result = 1;
                    msg.info = "Account already exists";
                }
                else
                {
                    accounts.Add(account, password);
                    msg.result = 0;
                    msg.info = "Registration successful";
                }
            }
            return msg;
        }

        public static LoginResultMsg Login(string account, string password)
        {
            LoginResultMsg msg = new LoginResultMsg();
            lock (locker)
            {
                if (!accounts.ContainsKey(account))
                {
                    msg.result = 2;
                    msg.info = "Account does not exist";
                }
                else if (accounts[account] != password)
                {
                    msg.result = 3;
                    msg.info = "Incorrect password";
                }
                else
                {
                    msg.result = 0;
                    msg.info = "Login successful";
                }
            }
            return msg;
        }
    }
}
