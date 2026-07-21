namespace GameLogin
{
	public class RegisterResultMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			RegisterResultMsg msg = message as RegisterResultMsg;
			LoginManager.GetInstance().RaiseRegisterResult(msg.result, msg.info);
		}
	}
}
