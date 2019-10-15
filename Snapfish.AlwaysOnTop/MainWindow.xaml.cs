using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
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
        public static SnapfishRecorder recorder = new SnapfishRecorder(); 
        public static Boolean recorderInitialized = false;
        public static SettingsContainer SettingsContainer = new SettingsContainer();
        public MainWindow()
        {
            InitializeComponent();
            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 205;
        }

        public async static void InitializeRecorder()
        {
            recorder.InstallDaemon();
            Thread.Sleep(500);
            recorder.CreateEchoSubscription();
            recorder.CreateBiomassSub();
            recorderInitialized = true;
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
                Task.Run(PostSnap).ContinueWith(displayMessageBoxOnSuccessfullSnapSent);
            else
                InitializeRecorder();
        }

        private void displayMessageBoxOnSuccessfullSnapSent(Task<string> task)
        {
            MessageBox.Show("Snap succesfully uploaded");
        }

        public static async Task<string> PostSnap()
        {
            List<Echogram> echos = recorder.CreateEchogramFileData().Result;
            List<StructIntegrationData> biomasses = recorder.CreateSubscribableFileData<StructIntegrationData>(EkSeriesDataSubscriptionType.Integration).Result;
            
            var packet = CreateSnapPacket(recorder, echos, biomasses);
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

//                var result = await client.PostAsync("http://10.218.157.115:5002/api/Snap/", stringContent);
                var result = await client.PostAsync("http://localhost:5000/api/Snap/", stringContent);
                // var resultContent = await result.Content.ReadAsStringAsync();
                System.Console.WriteLine("Snap posted to API. Response code from API: " + result.StatusCode);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = ConfigurationManager.AppSettings;  
            //Load from config if possible
            SettingsContainer.ipaddr = appSettings["ipaddr"] ?? "Not found";
            SettingsContainer.username = appSettings["username"] ?? "Not found";
            SettingsContainer.password = appSettings["password"] ?? "Not found";
            SettingsContainer.name = appSettings["name"] ?? "Not found";
            
            var dlg = new SettingsWindow
            {
                Owner = this,
                DataContext = SettingsContainer
            };
            dlg.ShowDialog();

            // Process data entered by user if dialog box is accepted
            if (dlg.DialogResult == true)
            {
                // For some reason validation doesnt work, no idea why
                Debug.WriteLine(dlg.IpAdress);
                SettingsContainer =  dlg.SettingsInfo;

            }
        }
    }
}