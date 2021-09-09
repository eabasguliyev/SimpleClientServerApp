using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Protocols;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 4444;

            string ipAddress = "127.0.0.100";

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);

            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                var isContinue = true;

                while (isContinue)
                {
                    socket.Connect(endPoint);

                    var strBuilder = new StringBuilder();

                    int count = 0;

                    byte[] dataBuffer = new byte[256];

                    var commandText = Console.ReadLine();

                    var protocol = new CommandProtocol();

                    if (commandText.Contains("Select") && commandText.Count(c => c == ';') == 1)
                        protocol.SelectionMode = true;

                    protocol.Query = commandText;


                    var jsonStr = JsonConvert.SerializeObject(protocol);


                    dataBuffer = Encoding.UTF8.GetBytes(jsonStr);

                    socket.Send(dataBuffer);

                    do
                    {
                        count = socket.Receive(dataBuffer);

                        strBuilder.Append(Encoding.UTF8.GetString(dataBuffer, 0, count));
                    } while (socket.Available > 0);

                    if (protocol.SelectionMode)
                    {
                        var response = JsonConvert.DeserializeObject<ResponseProtocol>(strBuilder.ToString());

                        foreach (var row in response.Rows)
                        {
                            foreach (var field in row.Fields)
                            {
                                Console.Write(field + " ");
                            }
                            Console.WriteLine();
                        }
                    }
                    else
                    {
                        Console.WriteLine(strBuilder);
                    }

                    Console.Write("Do you want to send another command? (y\\n)\n>> ");

                    isContinue = Console.ReadLine()[0] == 'y';

                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(true);
                }
                
                socket.Close();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Handled exception: {ex.Message}");
            }
        }
    }
}
