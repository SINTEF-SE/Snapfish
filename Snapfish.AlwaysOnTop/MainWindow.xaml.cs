using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Snapfish.AlwaysOnTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 205;
        }

        private void OnSnapButtonClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(PostSnap).ContinueWith(task => displayMessageBoxOnSuccessfullSnapSent());
        }

        private void displayMessageBoxOnSuccessfullSnapSent()
        {
            MessageBox.Show("Snap succesfully uploaded");
        }

        public static async Task<bool> PostSnap()
        {
            using (var client = new HttpClient())
            {
                var postTableContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("id", "1"), 
                });
                var result = await client.PostAsync("IPADDR/api/snap/upload", postTableContent);
                string resultContent = await result.Content.ReadAsStringAsync();
                System.Console.WriteLine(resultContent);
            }
            return true;
        }
    }
}