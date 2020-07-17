using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PubSubCommon
{
    public static class NetworkStreamUtils
    {
        public static async Task<string> ReadStringAsync(this NetworkStream stream)
        {
            var data = new byte[1024];
            var bytesReceived = await stream.ReadAsync(data, 0, data.Length);

            return Encoding.ASCII.GetString(data, 0, bytesReceived);
        }

        public static async Task WriteStringAsync(this NetworkStream stream, string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);

            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
