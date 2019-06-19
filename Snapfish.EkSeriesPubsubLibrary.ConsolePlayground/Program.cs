using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            EkSeriesSocketDaemon daemon = new EkSeriesSocketDaemon();
            Channel<Echogram> EchogramQueue = Channel.CreateBounded<Echogram>(new BoundedChannelOptions((1 << 8))
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
                    daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetApplicationName);
                }
                else if (key.StartsWith("a"))
                {
                    daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetChannelId);
                }
                else if (key.StartsWith("S"))
                {
                    daemon.SendSubscriptionRequest(EkSeriesRequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.Echogram);
                } else if (key.StartsWith("i"))
                {
                    daemon.CreateEchogramSubscription(ref EchogramQueue);
                } else if (key.StartsWith("j"))
                {
                    daemon.HandshakeWithEkSeriesDevice();
                    daemon.ConnectToRemoteEkDevice();
                    daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetApplicationName);
                    daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetChannelId);
                }
            }
        }
    }
}