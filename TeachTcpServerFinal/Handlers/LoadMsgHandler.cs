using System;
using System.IO;
using TeachTcpServerExercises2;

namespace GameSystem
{
    public class LoadMsgHandler : BaseHandler
    {
        private static readonly string SAVE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

        public override void MsgHandle()
        {
            LoadMsg msg = message as LoadMsg;
            LoadResultMsg result = new LoadResultMsg();

            try
            {
                string path = Path.Combine(SAVE_DIR, msg.account + ".sav");

                if (File.Exists(path))
                {
                    result.saveData = File.ReadAllBytes(path);
                    result.result = 0;
                    result.info = "Load successful";
                    Console.WriteLine("账号{0} 存档读取成功，大小:{1} bytes", msg.account, result.saveData.Length);
                }
                else
                {
                    result.saveData = null;
                    result.result = 1;
                    result.info = "No save file found";
                    Console.WriteLine("账号{0} 无存档记录", msg.account);
                }
            }
            catch (Exception e)
            {
                result.saveData = null;
                result.result = 2;
                result.info = "Load failed: " + e.Message;
                Console.WriteLine("账号{0} 存档读取失败: {1}", msg.account, e.Message);
            }

            _ = client.SendAsync(result);
        }
    }
}
