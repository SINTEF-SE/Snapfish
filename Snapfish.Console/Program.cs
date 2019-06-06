using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Snapfish.Application;
using Snapfish.BL.Models;

namespace Snapfish.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            SnapfishRecorder recorder = new SnapfishRecorder();
            recorder.InstallDaemon();
            Thread.Sleep(5000);
            recorder.AttachBufferToEchogramSubscription();
            while (true)
            {
                // CreateSampleDataSubscription
                string key = System.Console.ReadLine();
                if (key.StartsWith("c")) //HANDSHAKE
                {
                    List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                    if (echos != null)
                    {
                        //COol
                        System.Console.WriteLine("uuh");
                    }
                }  else if (key.StartsWith("b"))
                {
                    List<SampleDataContainerClass> sampleData = recorder.CreateSubscribableFileData<SampleDataContainerClass>(EkSeriesDataSubscriptionType.SampleData).Result;
                } else if (key.StartsWith("d"))
                {
                    List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                    List<SampleDataContainerClass> sampleData = recorder.CreateSubscribableFileData<SampleDataContainerClass>(EkSeriesDataSubscriptionType.SampleData).Result;
                    CreateTransmissableDataPacket(echos, sampleData);
                } 
            }
        }

        public static List<EchogramTransmissionPacket> CreateTransmissableDataPacket(List<Echogram> echos, List<SampleDataContainerClass> biomasses)
        {
            List<EchogramTransmissionPacket> snapData = new List<EchogramTransmissionPacket>();

            
            
            System.Console.WriteLine("Yo");

            return snapData;
        }
    }
}