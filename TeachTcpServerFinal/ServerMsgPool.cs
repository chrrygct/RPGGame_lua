using System;
using System.Collections.Generic;
using GameLogin;
using GameSystem;

namespace TeachTcpServerExercises2
{
    /// <summary>
    /// 服务端消息池：根据消息 ID 反射创建消息对象和处理器对象。
    /// 新增消息时只需在构造函数中加一行 Register 即可。
    /// </summary>
    public class ServerMsgPool
    {
        private Dictionary<int, Type> messages = new Dictionary<int, Type>();
        private Dictionary<int, Type> handlers = new Dictionary<int, Type>();

        public ServerMsgPool()
        {
            // 注册消息类型和对应的处理器类型
            Register(1001, typeof(RegisterMsg), typeof(RegisterMsgHandler));
            Register(1004, typeof(LoginMsg), typeof(LoginMsgHandler));
            Register(1002, typeof(HeartMsg), typeof(HeartMsgHandler));
            Register(1003, typeof(QuitMsg), typeof(QuitMsgHandler));
            Register(1007, typeof(SaveMsg), typeof(SaveMsgHandler));
            Register(1009, typeof(LoadMsg), typeof(LoadMsgHandler));
        }

        private void Register(int id, Type messageType, Type handlerType)
        {
            messages.Add(id, messageType);
            handlers.Add(id, handlerType);
        }

        public BaseMsg GetMessage(int id)
        {
            if (!messages.ContainsKey(id))
                return null;
            return Activator.CreateInstance(messages[id]) as BaseMsg;
        }

        public BaseHandler GetHandler(int id)
        {
            if (!handlers.ContainsKey(id))
                return null;
            return Activator.CreateInstance(handlers[id]) as BaseHandler;
        }
    }
}
