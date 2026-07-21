namespace GameLogin
{
	public class RegisterMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			RegisterMsg msg = message as RegisterMsg;
		}
	}
}
