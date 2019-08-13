using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
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
        public static SnapfishRecorder recorder = new SnapfishRecorder();
        public MainWindow()
        {
            InitializeComponent();
            recorder.InstallDaemon();
            recorder.CreateEchoSubscription();
            recorder.CreateBiomassSub();
            
            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 205;
        }

        private void OnSnapButtonClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(PostSnap).ContinueWith(task => displayMessageBoxOnSuccessfullSnapSent(task));
        }

        private void displayMessageBoxOnSuccessfullSnapSent(Task<string> task)
        {
            MessageBox.Show("Snap succesfully uploaded");
        }

        public static async Task<string> PostSnap()
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
    }
}