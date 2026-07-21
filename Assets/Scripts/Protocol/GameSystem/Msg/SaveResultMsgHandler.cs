namespace GameSystem
{
	public class SaveResultMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			SaveResultMsg msg = message as SaveResultMsg;
			// 通知等待中的请求（NetAsyncMgr.SendAndWaitAsync）
			NetAsyncMgr.Instance.NotifyReply(msg.GetID(), msg);
		}
	}
}
