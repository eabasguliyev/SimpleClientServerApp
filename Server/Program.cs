﻿using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Protocols;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 5555;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, port);

            using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            SqlConnection conn = new SqlConnection();

            conn.ConnectionString =
                "Data Source=condor\\SQLExpress;Initial Catalog=FavoriteMoviesDb;Integrated Security=True;";

            conn.Open();

            try
            {
                socket.Bind(endPoint);

                socket.Listen(100);

                Console.WriteLine("Ado.Net service is running. Waiting for connections...");


                while (true)
                {
                    var handler = socket.Accept();

                    Task.Run(() =>
                    {
                        while (true)
                        {
                            var strBuilder = new StringBuilder();

                            byte[] dataBuffer = new byte[256];

                            int count = 0;

                            do
                            {
                                count = handler.Receive(dataBuffer);

                                strBuilder.Append(Encoding.UTF8.GetString(dataBuffer, 0, count));
                            } while (handler.Available > 0);



                            var protocol = JsonConvert.DeserializeObject<CommandProtocol>(strBuilder.ToString());

                            Console.WriteLine(protocol.SelectionMode);
                            Console.WriteLine(protocol.Query);

                            // get result with ado.net


                            var command = conn.CreateCommand();

                            command.CommandText = protocol.Query;

                            try
                            {
                                if (protocol.SelectionMode)
                                {
                                    var reader = command.ExecuteReader();

                                    var response = new ResponseProtocol();


                                    var columnNames = new Row();

                                    for (int i = 0, length = reader.FieldCount; i < length; i++)
                                    {

                                        columnNames.Fields.Add(reader.GetName(i));
                                    }

                                    response.Rows.Add(columnNames);

                                    while (reader.Read())
                                    {
                                        var row = new Row();

                                        for (int i = 0, length = reader.FieldCount; i < length; i++)
                                        {
                                            row.Fields.Add(reader.GetValue(i));
                                        }

                                        response.Rows.Add(row);
                                    }

                                    reader.Close();

                                    var jsonStr = JsonConvert.SerializeObject(response);

                                    handler.Send(Encoding.UTF8.GetBytes(jsonStr));
                                }
                                else
                                {
                                    command.ExecuteNonQuery();

                                    handler.Send(Encoding.UTF8.GetBytes("Query Executed"));
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                handler.Send(Encoding.UTF8.GetBytes("Query is wrong."));
                            }
                        }
                    });
                }

            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);

                if (ex.ErrorCode == 10048)
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Handled exception: {ex.Message}");
            }
            finally
            {
                socket.Close();
                conn.Close();
            }
        }
    }
}
