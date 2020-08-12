using PubSubCommon;
using System;
// There's a newline here. I have never seen that in C# but you might have a reason for including it for logical separation.
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PubSub_Publisher // PascalCasing is generally standard for C# namespaces: https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-namespaces
{
    class Publisher
    {

        static async Task Main()
        {
            Thread.Sleep(1000);

            try
            {

                Int32 port = 9999;
                TcpClient client = new TcpClient("127.0.0.1", port);

                Console.WriteLine($"Publisher: {client.Client.LocalEndPoint}");

                string message = "CONNECTION-PUBLISHER";


                NetworkStream stream = client.GetStream();
                await stream.WriteStringAsync(message);

                _ = ReadFromBroker(client);

                string input = Console.ReadLine();
                while (!input.Equals("/quit"))
                {
                    await stream.WriteStringAsync(input);
                    input = Console.ReadLine();
                }

                await stream.WriteStringAsync(input);

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }

        static async Task ReadFromBroker(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            while(client.Connected)
            {
                var message = await stream.ReadStringAsync();
                Console.WriteLine(message);
            }

        }


    }

}
