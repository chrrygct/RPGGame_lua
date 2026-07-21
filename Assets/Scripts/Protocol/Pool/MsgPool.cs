using System;
using System.Collections.Generic;
using GameLogin;
using GameSystem;
public class MsgPool
{
	private Dictionary<int, Type> messsages = new Dictionary<int, Type>();
	private Dictionary<int, Type> handlers = new Dictionary<int, Type>();
	public MsgPool()
	{
		Register(1001, typeof(RegisterMsg), typeof(RegisterMsgHandler));
		Register(1005, typeof(RegisterResultMsg), typeof(RegisterResultMsgHandler));
		Register(1004, typeof(LoginMsg), typeof(LoginMsgHandler));
		Register(1006, typeof(LoginResultMsg), typeof(LoginResultMsgHandler));
		Register(1002, typeof(HeartMsg), typeof(HeartMsgHandler));
		Register(1003, typeof(QuitMsg), typeof(QuitMsgHandler));
		Register(1007, typeof(SaveMsg), typeof(SaveMsgHandler));
		Register(1008, typeof(SaveResultMsg), typeof(SaveResultMsgHandler));
		Register(1009, typeof(LoadMsg), typeof(LoadMsgHandler));
		Register(1010, typeof(LoadResultMsg), typeof(LoadResultMsgHandler));
	}
	private void Register(int id, Type messageType, Type handlerType)
	{
		messsages.Add(id, messageType);
		handlers.Add(id, handlerType);
	}
	public BaseMsg GetMessage(int id)
	{
		if (!messsages.ContainsKey(id))
			return null;
		return Activator.CreateInstance(messsages[id]) as BaseMsg;
	}
	public BaseHandler GetHandler(int id)
	{
		if (!handlers.ContainsKey(id))
			return null;
		return Activator.CreateInstance(handlers[id]) as BaseHandler;
	}
}
