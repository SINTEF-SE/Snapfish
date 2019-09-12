using System;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Snapfish.Application;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;
using System.Threading;
using System.Linq;
using System.Text;

namespace Snapfish.AlwaysOnTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static SnapfishRecorder recorder = null; 
        public static Boolean recorderInitialized = false;
        public MainWindow()
        {
            InitializeComponent();
/*            sendButton.IsEnabled = false;

            Thread viewerThread = new Thread(delegate ()
            {
                recorder.InstallDaemon();
                Thread.Sleep(7000);
                recorder.CreateEchoSubscription();
                recorder.CreateBiomassSub();
                recorderInitialized = true;
                sendButton.IsEnabled = true;
                //viewer = new SkeletalViewer.MainWindow();
                //viewer.Show();
                //System.Windows.Threading.Dispatcher.Run();
            });
            viewerThread.Start();
            */
            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 205;
        }


        public static async Task<Boolean> StartRecorder()
        {
            recorder = new SnapfishRecorder();
            recorder.InstallDaemon();
            Thread.Sleep(7000);
            recorder.CreateEchoSubscription();
            recorder.CreateBiomassSub();
            recorderInitialized = true;
            return true;
            //sendButton.IsEnabled = true;
        }



        private void OnSnapButtonClicked(object sender, RoutedEventArgs e)
        {
            if (recorderInitialized)
                Task.Run(PostSnap).ContinueWith(task => displayMessageBoxOnSuccessfullSnapSent(task));
            else
                Task.Run(StartRecorder);
        }

        private void displayMessageBoxOnSuccessfullSnapSent(Task<string> task)
        {
            MessageBox.Show("Snap succesfully uploaded");
        }

        public static async Task<string> PostSnap()
        {
            List<Echogram> echos = recorder.CreateEchogramFileData().Result;
            ///List<StructIntegrationData> biomass = recorder.CreateSubscribableFileData<StructIntegrationData>(EkSeriesDataSubscriptionType.Integration).Result;
            //var packet = CreateTransmissableDataPacket(recorder, echos, biomass);

            var packet = CreateSnapPacket(recorder, echos);
            UploadSnap(packet);


            string retval = "";
            using (var client = new HttpClient())
            {
                var postTableContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id", "1"), 
                });
                
                var result = await client.PostAsync("http://10.218.69.76:5002/api/EchogramInfos/", postTableContent);
                string resultContent = await result.Content.ReadAsStringAsync();
                retval = resultContent;
                System.Console.WriteLine(retval);
            }
            return retval;
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

//                var result = await client.PostAsync("http://10.218.157.115:5002/api/Snap/", stringContent);
                var result = await client.PostAsync("http://localhost:5000/api/Snap/", stringContent);
                // var resultContent = await result.Content.ReadAsStringAsync();
                System.Console.WriteLine("Snap posted to API. Response code from API: " + result.StatusCode);
            }
        }



        /*      public static async Task<string> PostSnap()
              {
                  List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                  List<StructIntegrationData> biomass = recorder.CreateSubscribableFileData<StructIntegrationData>(EkSeriesDataSubscriptionType.Integration).Result;
                  var packet = CreateTransmissableDataPacket(recorder, echos, biomass);


                  string retval = "";
                  using (var client = new HttpClient())
                  {
                      var postTableContent = new FormUrlEncodedContent(new[]
                      {
                          new KeyValuePair<string, string>("id", "1"),
                      });

                      var result = await client.PostAsync("http://10.218.69.76:5002/api/EchogramInfos/", postTableContent);
                      string resultContent = await result.Content.ReadAsStringAsync();
                      retval = resultContent;
                      System.Console.WriteLine(retval);
                  }
                  return retval;
              }
      */


        /*
                public static EchogramTransmissionPacket CreateTransmissableDataPacket(SnapfishRecorder recorder, List<Echogram> echos, List<StructIntegrationData> biomasses)
                {
                    EchogramTransmissionPacket packet = new EchogramTransmissionPacket
                    {
                        Latitude = recorder.GetLatitude(),
                        Longitude = recorder.GetLongitude(),
                        ApplicationName = recorder.GetApplicationName(),
                        ApplicationType = recorder.GetApplicationType(),
                        Echograms = echos,
                        Biomasses = biomasses
                    };
                    return packet;
                }
                */
    }
}