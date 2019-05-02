using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Snapfish.BL.Models
{
    public class EkSubscriptionListener
    {
        private struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }

        private static bool messageReceived = false;
        private UdpClient _client;
        
        public static void ReceiveMessages()
        {
            // Receive a message and write it to the console.
            IPEndPoint e = new IPEndPoint(IPAddress.Any, 8572);
            UdpClient u = new UdpClient(e);

            UdpState s = new UdpState {e = e, u = u};

            Console.WriteLine("listening for messages");
            u.BeginReceive(new AsyncCallback(ReceiveCallback), s);

            // Do some work while we wait for a message. For this example, we'll just sleep
            while (!messageReceived)
            {
                Thread.Sleep(100);
            }
        }
        
        private static void ReceiveCallback(IAsyncResult ar)
        {
            UdpClient u = ((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = ((UdpState)(ar.AsyncState)).e;

            byte[] receiveBytes = u.EndReceive(ar, ref e);
            string receiveString = Encoding.ASCII.GetString(receiveBytes);

            Console.WriteLine($"Received: {receiveString}");
            messageReceived = true;
        }

        
    }
}