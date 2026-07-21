namespace GameSystem
{
	public class LoadMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			LoadMsg msg = message as LoadMsg;
		}
	}
}
