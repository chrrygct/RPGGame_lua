using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TeachTcpServerExercises2
{
    class ServerSocket
    {
        //服务端Socket
        public Socket socket;
        //客户端连接的所有Socket
        public Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();

        //有待移除的客户端socket 避免 在foreach时直接从字典中移除 出现问题
        private List<ClientSocket> delList = new List<ClientSocket>();

        private bool isClose;

        //开启服务器端
        public void Start(string ip, int port, int num)
        {
            isClose = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(ipPoint);
            socket.Listen(num);
            // 启动异步接受客户端循环
            _ = AcceptLoopAsync();
        }

        //关闭服务器端
        public void Close()
        {
            isClose = true;
            lock (clientDic)
            {
                foreach (ClientSocket client in clientDic.Values)
                {
                    client.Close();
                }
                clientDic.Clear();
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }

        //异步接受客户端连入
        private async Task AcceptLoopAsync()
        {
            while (!isClose)
            {
                try
                {
                    Socket clientSocket = await socket.AcceptAsync();
                    ClientSocket client = new ClientSocket(clientSocket);
                    lock (clientDic)
                        clientDic.Add(client.clientID, client);
                    Console.WriteLine("客户端{0}连入成功", client.clientID);
                    // 启动该客户端的异步接收循环
                    _ = client.ReceiveLoopAsync();
                }
                catch (ObjectDisposedException)
                {
                    // 服务端关闭时 socket 被释放，忽略
                }
                catch (Exception e)
                {
                    Console.WriteLine("客户端连入报错" + e.Message);
                }
            }
        }

        public void Broadcast(BaseMsg info)
        {
            lock (clientDic)
            {
                foreach (ClientSocket client in clientDic.Values)
                {
                    _ = client.SendAsync(info);
                }
            }
                
        }

        //添加待移除的 socket内容（异步回调中调用，加锁保护并自动清理）
        public void AddDelSocket(ClientSocket socket)
        {
            lock (clientDic)
            {
                if (!delList.Contains(socket))
                    delList.Add(socket);
                CloseDelListSocket();
            }
        }

        ////判断有没有 断开连接的 把其 移除
        public void CloseDelListSocket()
        {
            //判断有没有 断开连接的 把其 移除
            for (int i = 0; i < delList.Count; i++)
                CloseClientSocket(delList[i]);
            delList.Clear();
        }

        //关闭客户端连接的 从字典中移除
        public void CloseClientSocket(ClientSocket socket)
        {
            lock (clientDic)
            {
                socket.Close();
                if (clientDic.ContainsKey(socket.clientID))
                {
                    clientDic.Remove(socket.clientID);
                    Console.WriteLine("客户端{0}主动断开连接了", socket.clientID);
                }
            }
        }
    }
}
