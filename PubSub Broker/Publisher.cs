using System; // Unused
using System.Collections.Generic; // Unused
using System.Text; // Unused

using System.Net.Sockets;
using Microsoft.VisualBasic.CompilerServices; // Unused

namespace PubSub_Broker
{
    class Publisher
    {
        
        private string topic;
        private TcpClient tcpClient;

        public Publisher(TcpClient tcp, string t)
        {
            tcpClient = tcp;
            topic = t;
        }

        public string GetTopic() // good place to use an expression-bodied function, in my opinion
        {
            return topic;
        }

        public TcpClient GetTcpClient() // this one, too
        {
            return tcpClient;
        }

    }

}
