using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Backend
{
    internal class Server
    {
        public class Request
        {
            public string RequestType { get; set; }
            public string[] Data { get; set; }
        }

        // 响应数据结构
        public class Response
        {
            public string Status { get; set; }
            public object Data { get; set; }
            public string Message { get; set; }
        }


        static Response HandleRequest(Request request)
        {

            if (request.RequestType == "GET_FILE_TREE")
            {
                FileSystem fs = new FileSystem();
                // 假设返回一个简单的文件树
                return new Response
                {
                    Status = "Success",
                    Data = fs.TraverseFileTree(),
                    Message = "Alllll files!!!!"
                };
            }

            if (request.RequestType == "CREATE_FILE")
            {

                FileSystem fs = new FileSystem();
                bool isDirectory = false;
                if (request.Data[0] == "-d")
                {
                    isDirectory = true;
                }
                try
                {
                    fs.CreateFileObject(request.Data[1], isDirectory);
                }
                catch (Exception e)
                {
                    if (e is FileNameConflictException)
                    {
                        return new Response
                        {
                            Status = "Error",
                            Data = "",
                            Message = "文件名称重复"
                        };
                    }
                    if (e is RootDirectoryLimitExceededException)
                    {
                        return new Response
                        {
                            Status = "Error",
                            Data = "",
                            Message = "根目录下文件数量限制"
                        };
                    }
                }

                return new Response
                {
                    Status = "Success",
                    Data = "",
                    Message = "Alllll files!!!!"
                };
            }


            if (request.RequestType == "DELETE_FILE")
            {
                FileSystem fs = new FileSystem();
                try
                {
                    fs.DeleteFileObject(request.Data[0]);
                }
                catch (Exception e)
                {
                    if (e is FileObjectPathNotExistException)
                    {
                        return new Response
                        {
                            Status = "Error",
                            Message = $"文件不存在"
                        };
                    }
                }

            }

            return new Response
            {
                Status = "Error",
                Message = $"Unsupported request type."
            };
        }

        public static void ListenPort()
        {
            int port = 5000; // 监听的端口号
            TcpListener server = new TcpListener(IPAddress.Loopback, port);
            server.Start();
            Console.WriteLine($"Server started at 127.0.0.1:{port}");
            while (true)
            {
                var client = server.AcceptTcpClient();
                var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // 读取客户端发送的消息
                string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                var request = JsonSerializer.Deserialize<Request>(receivedData);

                Console.WriteLine($"Received: {request.RequestType}");

                var response = HandleRequest(request);

                // 处理消息并返回响应
                Console.WriteLine($"Responsed status: {response.Status}");
                var responseJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
                stream.Write(responseJson, 0, responseJson.Length);

                client.Close();

            }


            Console.WriteLine("Press Enter to exit.");
            Console.ReadLine();
        }
    }
}
