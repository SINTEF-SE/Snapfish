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
                    //CreateTransmissableDataPacket(recorder, echos, sampleData);
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
                    var packet = CreateSnapPacket(recorder, echos);
                    UploadSnap(packet);
                }
            }
        }

        private static SnapPacket CreateSnapPacket(SnapfishRecorder recorder, List<Echogram> echograms)
        {
            var slices = new Slice[echograms.Count];
            foreach (var i in Enumerable.Range(0, echograms.Count))
            {
                slices[i] = new Slice
                {
                    Data = echograms[i].EchogramArray.nEchogramElement,
                    DataLength = echograms[i].EchogramArray.nEchogramElement.Length,
                    Range = echograms[i].EchogramHeader.range,
                    RangeStart = echograms[i].EchogramHeader.rangeStart,
                    Timestamp = echograms[i].EchogramHeader.dlTime
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

                var result = await client.PostAsync("http://localhost:5000/api/Snap/", stringContent);
                // var resultContent = await result.Content.ReadAsStringAsync();
                // System.Console.WriteLine(resultContent);
                System.Console.WriteLine("Response code from API: " + result.StatusCode);
            }
        }
    }
}