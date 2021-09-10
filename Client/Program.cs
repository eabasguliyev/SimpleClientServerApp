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
            int port = 5555;
            
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);

            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                var isContinue = true;

                socket.Connect(endPoint);

                while (isContinue)
                {

                    var strBuilder = new StringBuilder();

                    int count = 0;

                    byte[] dataBuffer = new byte[256];

                    var commandText = Console.ReadLine();

                    var protocol = new CommandProtocol();

                    if (commandText.Contains("Select"))
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

             
                }
                
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Handled exception: {ex.Message}");
            }
            finally
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
        }
    }
}
