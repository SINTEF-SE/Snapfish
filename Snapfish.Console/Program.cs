using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Snapfish.Application;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.Console
{
    class Program
    {
        private static void Main(string[] args)
        {
            SnapfishRecorder recorder = new SnapfishRecorder();
            recorder.InstallDaemon();
            Thread.Sleep(7000);
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
                }
                else if (key.StartsWith("b"))
                {
                    List<SampleDataContainerClass> sampleData = recorder.CreateSubscribableFileData<SampleDataContainerClass>(EkSeriesDataSubscriptionType.SampleData).Result;
                }
                else if (key.StartsWith("d"))
                {
                    List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                    List<SampleDataContainerClass> sampleData = recorder.CreateSubscribableFileData<SampleDataContainerClass>(EkSeriesDataSubscriptionType.SampleData).Result;
                }
                else if (key.StartsWith("e"))
                {
                    recorder.CreateEchoSubscription();
                } else if (key.StartsWith("p"))
                {
                    recorder.CreateBiomassSub();
                }
                else if (key.StartsWith("f"))
                {
                    var echos = recorder.CreateEchogramFileData().Result;
                    List<StructIntegrationData> biomasses = recorder.CreateSubscribableFileData<StructIntegrationData>(EkSeriesDataSubscriptionType.Integration).Result;
                    var packet = CreateSnapPacket(recorder, echos, biomasses);
                    UploadSnap(packet);
                } else if (key.StartsWith("v"))
                {
                    recorder.DisconnectFromConnectedEkDevice();
                }
            }
        }

        private static SnapPacket CreateSnapPacket(SnapfishRecorder recorder, List<Echogram> echograms, List<StructIntegrationData> biomasses)
        {

            // If the biomass and echo subscriptions are not started at the exact same time, the counts will differ
            // This makes sure there are no slices without biomass or echo data
            var nSlices = Math.Min(echograms.Count, biomasses.Count);
            
            var slices = new Slice[nSlices];
            foreach (var i in Enumerable.Range(0, nSlices))
            {
                var index = nSlices - 1 - i;  // Makes sure the biomasses and echo data are lined up if one started before the other
                slices[index] = new Slice
                {
                    Data = echograms[index].EchogramArray.nEchogramElement,
                    DataLength = echograms[index].EchogramArray.nEchogramElement.Length,
                    Range = echograms[index].EchogramHeader.range,
                    RangeStart = echograms[index].EchogramHeader.rangeStart,
                    Timestamp = echograms[index].EchogramHeader.dlTime,
                    Biomass = Convert.ToInt32(biomasses[index].IntegrationDataBody.dSa) // Todo: Set parameters for biomass subscription based on settings in connected EK80
                };
            }
            
            return new SnapPacket
            {
               OwnerId = 212, // TODO: Handle OwnerId when posting new Snap
               Timestamp = DateTime.Now,
               Latitude = recorder.GetLatitude(),
               Longitude = recorder.GetLongitude(),
               Slices = slices,
               NumberOfSlices = slices.Length,
               SliceHeight = slices[0].DataLength
            };
        }

        public static async void UploadSnap(SnapPacket packet)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(packet);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var result = await client.PostAsync("http://129.242.16.123:37789/api/Snap/", stringContent);
                //                var result = await client.PostAsync("http://localhost:5000/api/Snap/", stringContent);
                //                var result = await client.PostAsync("http://10.218.157.115:5002/api/Snap/", stringContent);
                // var resultContent = await result.Content.ReadAsStringAsync();
                System.Console.WriteLine("Snap posted to API. Response code from API: " + result.StatusCode);
            }
        }
    }
}