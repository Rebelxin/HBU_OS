using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backend
{
    internal class Server
    {

        public static void ListenPort()
        {
            int port = 5000; // 监听的端口号
            TcpListener server = new TcpListener(IPAddress.Loopback, port);
            server.Start();
            Console.WriteLine($"Server started at 127.0.0.1:{port}");

            Task.Run(() =>
            {
                while (true)
                {
                    var client = server.AcceptTcpClient();
                    Task.Run(() =>
                    {
                        var stream = client.GetStream();
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        // 读取客户端发送的消息
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {receivedMessage}");

                        // 处理消息并返回响应
                        string response = $"Processed: {receivedMessage} at {DateTime.Now}";
                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                        stream.Write(responseBytes, 0, responseBytes.Length);

                        client.Close();
                    });
                }
            });

            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
