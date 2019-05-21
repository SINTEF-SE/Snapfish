using System.Collections.Generic;
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
        private static Channel<SampleDataContainerClass> _biomassBoundedBuffer;
        EkSeriesSocketDaemon daemon = new EkSeriesSocketDaemon();

        private string _applicationName;
        private string _applicationType;
        private string _applicationDescription;
        private string _applicationVersion;
        private string _latitude;
        private string _longitude;

        public SnapfishRecorder()
        {
            _boundedBuffer = Channel.CreateBounded<Echogram>(new BoundedChannelOptions(_bufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
            
            _biomassBoundedBuffer = Channel.CreateBounded<SampleDataContainerClass>(new BoundedChannelOptions(_bufferSize)
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

        public async Task<List<Echogram>> /*List<Echogram>*/ CreateEchogramFileData()
        {
            /*
             * Something different
             */
            List<Echogram> echos = await Task.Run(() => ConsumeChannel(_boundedBuffer.Reader)).ContinueWith(task => CreateEchogramFile(task));
            
            /*Task<List<Echogram>> consumeTask = Consume(_boundedBuffer.Reader);
            consumeTask.Wait();
            List<Echogram> echos = consumeTask.Result;*/
            return echos;
        }

        public async Task<List<T>> CreateSubscribableFileData<T>()
        {
            List<T> retval = await Task.Run(() => ConsumeChannel<T>(_biomassBoundedBuffer.Reader)).ContinueWith(task => CreateSubscribableFile(task));
        }

        static List<Echogram> CreateEchogramFile(Task<List<Echogram>> task)
        {
            List<Echogram> EchoYolo = task.Result;
            
            //Do stuff, then return the 'file'
            // Not sure what to do
            return EchoYolo;
        }

        static List<T> CreateSubscribableFile<T>(Task<List<T>> task)
        {
            List<T> retval = task.Result;

            return retval;
        }
        
        public static List<T> ConsumeChannel<T>(ChannelReader<T> channelReader)
        {
            List<T> retval = new List<T>();
            int i = 0;
            while (channelReader.TryRead(out T item))
            {
                retval.Add(item);
                //TODO: Dont do this, but you know. Its kinda of how we got to do this atm
                if (++i > _bufferSize)
                {
                    break;
                }
            }
            return retval;
        } 
        
        public static List<Echogram> ConsumeChannel(ChannelReader<Echogram> channel)
        {
            List<Echogram> retval = new List<Echogram>();
            int i = 0;
            while (channel.TryRead(out Echogram item))
            {
                retval.Add(item);
                //TODO: Dont do this, but you know. Its kinda of how we got to do this atm
                if (++i > _bufferSize)
                {
                    break;
                }
            }
            return retval;
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
            catch (ChannelClosedException)
            {
            }
        }

    }
}