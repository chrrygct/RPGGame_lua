using System;
using System.IO;
using TeachTcpServerExercises2;

namespace GameSystem
{
    public class SaveMsgHandler : BaseHandler
    {
        private static readonly string SAVE_DIR = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "saves");

        public override void MsgHandle()
        {
            SaveMsg msg = message as SaveMsg;
            SaveResultMsg result = new SaveResultMsg();

            try
            {
                if (!Directory.Exists(SAVE_DIR))
                    Directory.CreateDirectory(SAVE_DIR);

                string path = Path.Combine(SAVE_DIR, msg.account + ".sav");
                File.WriteAllBytes(path, msg.saveData ?? Array.Empty<byte>());

                result.result = 0;
                result.info = "Save successful";
                Console.WriteLine("账号{0} 存档保存成功，大小:{1} bytes", msg.account, msg.saveData?.Length ?? 0);
            }
            catch (Exception e)
            {
                result.result = 1;
                result.info = "Save failed: " + e.Message;
                Console.WriteLine("账号{0} 存档保存失败: {1}", msg.account, e.Message);
            }

            _ = client.SendAsync(result);
        }
    }
}
