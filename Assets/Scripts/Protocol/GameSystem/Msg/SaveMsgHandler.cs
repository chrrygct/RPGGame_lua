namespace GameSystem
{
	public class SaveMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			SaveMsg msg = message as SaveMsg;
		}
	}
}
