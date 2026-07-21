namespace GameLogin
{
	public class LoginResultMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			LoginResultMsg msg = message as LoginResultMsg;
			LoginManager.GetInstance().RaiseLoginResult(msg.result, msg.info);
		}
	}
}
