using System.Collections.Generic;
using System.Net;
using System.Threading.Channels;
using System.Threading.Tasks;
using Snapfish.BL.Models;
using Snapfish.EkSeriesPubsubLibrary;

namespace Snapfish.Application
{
    public class SnapfishRecorder
    {
        private const int _bufferSize = 1 << 8;  
        private static Channel<Echogram> _boundedBuffer;
        EkSeriesSocketDaemon daemon = new EkSeriesSocketDaemon();

        public SnapfishRecorder()
        {
            _boundedBuffer = Channel.CreateBounded<Echogram>(new BoundedChannelOptions(_bufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
        }

        public void InstallDaemon()
        {
            daemon.HandshakeWithEkSeriesDevice();
            daemon.ConnectToRemoteEkDevice();
            daemon.SendParameterRequestToEkSeriesDevice(ParameterRequestType.GET_PARAMETER, ParameterType.GetApplicationName);
            daemon.SendParameterRequestToEkSeriesDevice(ParameterRequestType.GET_PARAMETER, ParameterType.GetChannelId);
        }

        public void AttachBufferToEchogramSubscription()
        {
            daemon.CreateEchogramSubscription(ref _boundedBuffer);
        }

        public List<Echogram> CreateEchogramFileData()
        {
            Task<List<Echogram>> consumeTask = Consume(_boundedBuffer.Reader);
            consumeTask.Wait();
            List<Echogram> echos = consumeTask.Result;
            return echos;
        }

        public static async Task ConsumeChannelData(ChannelReader<Echogram> c)
        {
            try
            {
                List<Echogram> retval = new List<Echogram>();
                while (true)
                {
                    Echogram item = await c.ReadAsync();
                    retval.Add(item);
                    
                }
            }
            catch (ChannelClosedException) {}
        }
        
        private static async Task<List<Echogram>> Consume(ChannelReader<Echogram> c)
        {
            List<Echogram> retval = new List<Echogram>();
            while (await c.WaitToReadAsync())
            {
                while (c.TryRead(out Echogram item))
                {
                    retval.Add(item);
                    if (retval.Count > 1000) // Jic. I havent really tried the channel API
                    {
                        goto end;
                    }
                }
            }
            end:
            return retval;
        }
    }
}