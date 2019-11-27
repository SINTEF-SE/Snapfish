using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Snapfish.Application;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.AlwaysOnTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static SnapfishRecorder recorder;
        public static Boolean recorderInitialized = false;
        public static SettingsContainer SettingsContainer = new SettingsContainer();

        public MainWindow()
        {
            InitializeComponent();
            Left = SystemParameters.PrimaryScreenWidth - Width - 205;
        }

        public async static void InitializeRecorder()
        {
            if (!ParametersLooksValid())
            {
                MessageBox.Show("Invalid credentials. Please enter correct credentials in the settings menu");
                return;
            }

            recorder = new SnapfishRecorder(SettingsContainer.ekIpAddr);
            recorder.InstallDaemon(SettingsContainer.username, SettingsContainer.password);
            Thread.Sleep(500);
            recorder.CreateEchoSubscription();
            recorder.CreateBiomassSub();
            recorderInitialized = true;
        }

        public static async Task<bool?> StartRecorder()
        {
            if (!ParametersLooksValid())
            {
                MessageBox.Show("Invalid credentials. Please enter correct credentials in the settings menu");
                return null;
            }

            recorder = new SnapfishRecorder(SettingsContainer.ekIpAddr);
            recorder.InstallDaemon(SettingsContainer.username, SettingsContainer.password);
            Thread.Sleep(7000);
            recorder.CreateEchoSubscription();
            recorder.CreateBiomassSub();
            recorderInitialized = true;
            return true;
            //sendButton.IsEnabled = true;
        }

        private static bool ParametersLooksValid()
        {
            var appSettings = ConfigurationManager.AppSettings;
            SettingsContainer.ekIpAddr = appSettings["ekIpAddr"] ?? "Not Set";
            SettingsContainer.username = appSettings["username"] ?? "Not found";
            SettingsContainer.password = appSettings["password"] ?? "Not found";
            SettingsContainer.name = appSettings["name"] ?? "Not found";
            return !SettingsContainer.ekIpAddr.Equals("Not Set") && !SettingsContainer.ekIpAddr.Equals("0.0.0.0") && !SettingsContainer.username.Equals("Not found") &&
                   !SettingsContainer.password.Equals("Not found") && !SettingsContainer.name.Equals("Not found");
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
            MessageBox.Show("Snap successfully uploaded");
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

                var result = await client.PostAsync("http://esushi.no:37789/api/EchogramInfos/", postTableContent);
                string resultContent = await result.Content.ReadAsStringAsync();
                retval = resultContent;
                Console.WriteLine(retval);
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
                var index = nSlices - 1 - i; // Makes sure the biomasses and echo data are lined up if one started before the other
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
                OwnerId = 1, // TODO: Handle OwnerId when posting new Snap
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
                var result = await client.PostAsync("http://esushi.no:37789/api/Snap/", stringContent);
                // var resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine("Snap posted to API. Response code from API: " + result.StatusCode);
            }
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettings = ConfigurationManager.AppSettings;

            //Load from config if possible
            SettingsContainer.ekIpAddr = appSettings["ekIpAddr"] ?? "Not Set";
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
            var storeSettings = configuration.AppSettings;
            if (dlg.DialogResult == true)
            {
                // For some reason validation doesnt work, no idea why
                Debug.WriteLine(dlg.IpAdress);
                SettingsContainer = dlg.SettingsInfo;
                storeSettings.Settings["ekIpAddr"].Value = SettingsContainer.ekIpAddr;
                storeSettings.Settings["username"].Value = SettingsContainer.username;
                storeSettings.Settings["password"].Value = SettingsContainer.password;
                storeSettings.Settings["name"].Value = SettingsContainer.name;
                configuration.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }
    }
}