using System;

namespace TeachTcpServerExercises2
{
    class Program
    {
        public static ServerSocket socket;
        static void Main(string[] args)
        {
            socket = new ServerSocket();
            socket.Start("127.0.0.1", 8080, 1024);
            Console.WriteLine("服务器开启成功，输入 Quit 关闭服务器");
            while (true)
            {
                string input = Console.ReadLine();
                if (input == "Quit")
                {
                    socket.Close();
                    break;
                }
            }
        }
    }
}
