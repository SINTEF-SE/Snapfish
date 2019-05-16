using System;
using System.Collections.Generic;
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
            Thread.Sleep(1000);
            recorder.AttachBufferToEchogramSubscription();
            while (true)
            {
                
                string key = System.Console.ReadLine();
                if (key.StartsWith("c")) //HANDSHAKE
                {
                    List<Echogram> echos = recorder.CreateEchogramFileData().Result;
                    if (echos != null)
                    {
                        //COol
                        System.Console.WriteLine("uuh");
                    }
                }   
            }
        }
    }
}