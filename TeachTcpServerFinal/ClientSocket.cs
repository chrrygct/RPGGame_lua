using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeachTcpServerExercises2
{
    public class ClientSocket
    {
        private static int CLIENT_BEGIN_ID = 1;
        public int clientID;
        public Socket socket;

        // 消息池（所有客户端共享同一份注册表）
        private static ServerMsgPool msgPool = new ServerMsgPool();

        //用于处理分包时 缓存的 字节数组 和 字节数组长度
        private byte[] cacheBytes = new byte[1024 * 1024];
        private int cacheNum = 0;

        //异步接收缓冲区
        private byte[] receiveBuffer = new byte[1024 * 5];

        //上一次收到消息的时间
        private long frontTime = -1;
        //超时时间
        private static int TIME_OUT_TIME = 10;

        public ClientSocket(Socket socket)
        {
            this.clientID = CLIENT_BEGIN_ID;
            this.socket = socket;
            ++CLIENT_BEGIN_ID;
            // 连接成功即记录时间，防止客户端不发心跳永不断开
            frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            // 启动异步心跳超时检测循环
            _ = CheckTimeOutLoopAsync();
        }

        /// <summary>
        /// 异步心跳超时检测循环：不与收消息耦合，每 5 秒独立检查一次
        /// </summary>
        private async Task CheckTimeOutLoopAsync()
        {
            while (Connected)
            {
                await Task.Delay(5000);
                if (frontTime != -1 &&
                    DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
                {
                    Console.WriteLine("客户端{0}心跳超时，主动断开", clientID);
                    Program.socket.AddDelSocket(this);
                    return;
                }
            }
        }

        /// <summary>
        /// 是否是连接状态
        /// </summary>
        public bool Connected => socket.Connected;

        /// <summary>
        /// 记录心跳时间（由 HeartMsgHandler 调用）
        /// </summary>
        public void RecordHeartbeat()
        {
            frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
        }

        //我们应该封装一些方法
        //关闭
        public void Close()
        {
            if(socket != null)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        //发送（异步 async/await）
        public async Task SendAsync(BaseMsg info)
        {
            if (Connected)
            {
                try
                {
                    byte[] data = info.Writing();
                    await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);
                }
                catch (Exception e)
                {
                    Console.WriteLine("发消息出错" + e.Message);
                    Program.socket.AddDelSocket(this);
                }
            }
            else
                Program.socket.AddDelSocket(this);
        }

        //异步接收循环
        public async Task ReceiveLoopAsync()
        {
            while (Connected)
            {
                try
                {
                    int receiveNum = await socket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer), SocketFlags.None);

                    if (receiveNum == 0 || !Connected)
                    {
                        Program.socket.AddDelSocket(this);
                        return;
                    }
                    HandleReceiveMsg(receiveBuffer, receiveNum);
                }
                catch (SocketException)
                {
                    Program.socket.AddDelSocket(this);
                    return;
                }
                catch (ObjectDisposedException)
                {
                    // socket 已关闭，忽略
                    return;
                }
            }
        }

        //处理接受消息 分包、黏包问题的方法
        private void HandleReceiveMsg(byte[] receiveBytes, int receiveNum)
        {
            int msgID = 0;
            int msgLength = 0;
            int nowIndex = 0;

            //收到消息时 应该看看 之前有没有缓存的 如果有的话 我们直接拼接到后面
            receiveBytes.CopyTo(cacheBytes, cacheNum);
            cacheNum += receiveNum;

            while (true)
            {
                //每次将长度设置为-1 是避免上一次解析的数据 影响这一次的判断
                msgLength = -1;
                //处理解析一条消息
                if (cacheNum - nowIndex >= 8)
                {
                    //解析ID
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    //解析长度
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                }

                if (cacheNum - nowIndex >= msgLength && msgLength != -1)
                {
                    //解析消息体：通过消息池根据 ID 获取消息对象并反序列化
                    BaseMsg baseMsg = msgPool.GetMessage(msgID);
                    if (baseMsg != null)
                    {
                        baseMsg.Reading(cacheBytes, nowIndex);
                        //通过消息池获取对应的处理器，注入消息和客户端连接后执行
                        BaseHandler handler = msgPool.GetHandler(msgID);
                        if (handler != null)
                        {
                            handler.message = baseMsg;
                            handler.client = this;
                            handler.MsgHandle();
                        }
                    }
                    nowIndex += msgLength;
                    if (nowIndex == cacheNum)
                    {
                        cacheNum = 0;
                        break;
                    }
                }
                else
                {
                    //如果不满足 证明有分包 
                    //那么我们需要把当前收到的内容 记录下来
                    //有待下次接受到消息后 再做处理
                    //receiveBytes.CopyTo(cacheBytes, 0);
                    //cacheNum = receiveNum;
                    //如果进行了 id和长度的解析 但是 没有成功解析消息体 那么我们需要减去nowIndex移动的位置
                    if (msgLength != -1)
                        nowIndex -= 8;
                    //就是把剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                    Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                    cacheNum = cacheNum - nowIndex;
                    break;
                }
            }

        }

    }
}
