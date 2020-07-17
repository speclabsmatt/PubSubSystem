using System;
using System.Collections.Generic;
using System.Text;

using System.Net.Sockets;
using Microsoft.VisualBasic.CompilerServices;

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

        public string GetTopic()
        {
            return topic;
        }

        public TcpClient GetTcpClient()
        {
            return tcpClient;
        }

    }

}
