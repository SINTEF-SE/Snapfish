using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Snapfish.BL.Models;

namespace Snapfish.EkSeriesPubsubLibrary.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            EkSeriesSocketDaemon daemon = new EkSeriesSocketDaemon();
            Channel<Echogram> EchogramQueue = Channel.CreateBounded<Echogram>(new BoundedChannelOptions((1 << 12))
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
            while (true)
            {
                string key = Console.ReadLine();
                if (key.StartsWith("h")) //HANDSHAKE
                {
                    daemon.HandshakeWithEkSeriesDevice();
                }
                else if (key.StartsWith("c"))
                {
                    daemon.ConnectToRemoteEkDevice();
                }
                else if (key.StartsWith("p"))
                {
                    daemon.SendParameterRequestToEkSeriesDevice(ParameterRequestType.GET_PARAMETER, ParameterType.GetApplicationName);
                }
                else if (key.StartsWith("a"))
                {
                    daemon.SendParameterRequestToEkSeriesDevice(ParameterRequestType.GET_PARAMETER, ParameterType.GetChannelId);
                }
                else if (key.StartsWith("S"))
                {
                    daemon.SendSubscriptionRequest(Ek80RequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.Echogram);
                } else if (key.StartsWith("i"))
                {
                    daemon.CreateEchogramSubscription(ref EchogramQueue);
                }
            }
        }
    }
}