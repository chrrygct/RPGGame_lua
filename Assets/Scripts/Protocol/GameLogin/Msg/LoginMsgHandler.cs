namespace GameLogin
{
	public class LoginMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			LoginMsg msg = message as LoginMsg;
		}
	}
}
