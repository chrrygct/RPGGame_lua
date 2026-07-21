namespace GameSystem
{
	public class LoadResultMsgHandler : BaseHandler
	{
		public override void MsgHandle()
		{
			LoadResultMsg msg = message as LoadResultMsg;
			// 通知等待中的请求（NetAsyncMgr.SendAndWaitAsync）
			NetAsyncMgr.Instance.NotifyReply(msg.GetID(), msg);
		}
	}
}
