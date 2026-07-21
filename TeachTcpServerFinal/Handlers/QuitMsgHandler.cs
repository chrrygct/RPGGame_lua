using TeachTcpServerExercises2;

namespace GameSystem
{
    public class QuitMsgHandler : BaseHandler
    {
        public override void MsgHandle()
        {
            QuitMsg msg = message as QuitMsg;
            Program.socket.AddDelSocket(client);
        }
    }
}
