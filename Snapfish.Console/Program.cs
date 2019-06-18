using System;
using System.Collections.Generic;
using System.Net.Http;
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
            Thread.Sleep(7000);
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
                } else if (key.StartsWith("f"))
                {
                    List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                    List<TargetsIntegration> biomass = recorder.CreateSubscribableFileData<TargetsIntegration>(EkSeriesDataSubscriptionType.TargetsIntegration).Result;
                    CreateTransmissableDataPacket(echos, biomass);
                }
            }
        }

        public static List<EchogramTransmissionPacket> CreateTransmissableDataPacket(List<Echogram> echos, List<TargetsIntegration> biomasses)
        {
            List<EchogramTransmissionPacket> snapData = new List<EchogramTransmissionPacket>();
            
            System.Console.WriteLine("yoyo");

            return snapData;
        }

        public static List<EchogramTransmissionPacket> CreateTransmissableDataPacket(List<Echogram> echos, List<SampleDataContainerClass> biomasses)
        {
            List<EchogramTransmissionPacket> snapData = new List<EchogramTransmissionPacket>();
            
            
            
            System.Console.WriteLine("Yo");

            return snapData;
        }
        
        public async void UploadSnap(List<EchogramTransmissionPacket> packets)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                                   {
                                       new KeyValuePair<string, string>("", "upload")
                                   });
                var result = await client.PostAsync("/api/snap/upload", content);
                string resultContent = await result.Content.ReadAsStringAsync();
                System.Console.WriteLine(resultContent);
            }
        }
    }
}