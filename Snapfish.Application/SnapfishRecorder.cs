using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;
using Snapfish.EkSeriesPubsubLibrary;

namespace Snapfish.Application
{
    public class SnapfishRecorder
    {
        private const int BufferSize = 1 << 8;
        private static Channel<Echogram> _boundedBuffer;
        private static Channel<SampleDataContainerClass> _sampleDataBoundedBuffer;
        private static Channel<TargetsIntegration> _targetsBiomassBoundedBuffer;
        public static Channel<StructIntegrationData> _biomassBoundedBuffer;
        private readonly EkSeriesSocketDaemon _daemon = new EkSeriesSocketDaemon("10.218.68.118");

        private string _applicationName;
        private string _applicationType;
        private string _applicationDescription;
        private string _applicationVersion;
        private string _latitude;
        private string _longitude;

        public SnapfishRecorder()
        {
            _boundedBuffer = Channel.CreateBounded<Echogram>(new BoundedChannelOptions(BufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
            
            _sampleDataBoundedBuffer = Channel.CreateBounded<SampleDataContainerClass>(new BoundedChannelOptions(BufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });

            _targetsBiomassBoundedBuffer = Channel.CreateBounded<TargetsIntegration>(new BoundedChannelOptions(BufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
            
            _biomassBoundedBuffer = Channel.CreateBounded<StructIntegrationData>(new BoundedChannelOptions(BufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
        }

        public void InstallDaemon()
        {
            _daemon.HandshakeWithEkSeriesDevice();
            _daemon.ConnectToRemoteEkDevice();
            _daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetApplicationName);
            _daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.GetChannelId);
            MakeDaemonFetchAttachGeoData();
        }

        public void DisconnectFromConnectedEkDevice()
        {
            _daemon.DisconnectFromRemoteEkDevice();
        }
        
        public void CreateEchoSubscription()
        {
            _daemon.CreateEchogramSubscription(ref _boundedBuffer);
        }

        public void CreateBiomassSub()
        {
            _daemon.CreateBiomassSubscription(ref _biomassBoundedBuffer);
        }
        
        public void AttachBufferToEchogramSubscription()
        {
            _daemon.CreateEchogramSubscription(ref _boundedBuffer);
            //_daemon.CreateSampleDataSubscription(ref _sampleDataBoundedBuffer);
            _daemon.CreateBiomassSubscription(ref _biomassBoundedBuffer);
        }

        public void MakeDaemonFetchAttachGeoData()
        {
            _daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.Latitude);
            _daemon.SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest.GET_PARAMETER, EkSeriesParameterType.Longitude);
        }
        
        public string GetLatitude()
        {
            return _daemon.Latitude;
        }
        
        public string GetLongitude()
        {
            return _daemon.Longitude;
        }

        public string GetApplicationName()
        {
            return _daemon.ApplicationName;
        }

        public string GetApplicationType()
        {
            return _daemon.ApplicationType;
        }
        
        public async Task<List<Echogram>> CreateEchogramFileData()
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

        
        
        public async Task<List<T>> CreateSubscribableFileData<T>(EkSeriesDataSubscriptionType type)
        {
            List<T> retval = new List<T>();
            switch (type)
            {
                case EkSeriesDataSubscriptionType.BottomDetection:
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetection:
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp:
                    break;
                case EkSeriesDataSubscriptionType.SampleData:
                    retval = await Task.Run(() => ConsumeChannel(_sampleDataBoundedBuffer.Reader as ChannelReader<T>)).ContinueWith(task => CreateSubscribableFile(task));
                    break;
                case EkSeriesDataSubscriptionType.Echogram:
                    break;
                case EkSeriesDataSubscriptionType.TargetsEchogram:
                    break;
                case EkSeriesDataSubscriptionType.Integration:
                    retval = await Task.Run(() => ConsumeChannel(_biomassBoundedBuffer.Reader as ChannelReader<T>)).ContinueWith(task => CreateSubscribableFile(task));
                    break;
                case EkSeriesDataSubscriptionType.IntegrationChirp:
                    break;
                case EkSeriesDataSubscriptionType.TargetsIntegration:
                    retval = await Task.Run(() => ConsumeChannel(_biomassBoundedBuffer.Reader as ChannelReader<T>)).ContinueWith(task => CreateSubscribableFile(task));
                    break;
                case EkSeriesDataSubscriptionType.NoiseSpectrum:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return retval;
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
        
        public static List<T> ConsumeChannel<T>(ChannelReader<T> channelReader, int limit)
        {
            List<T> retval = new List<T>();
            int i = 0;
            while (channelReader.TryRead(out T item))
            {
                retval.Add(item);
                if (++i > limit)
                {
                    break;
                }
            }
            return retval;
        } 
        
        /* For some reason this doesnt work */
        public static List<T> ConsumeChannel<T>(ChannelReader<T> channelReader)
        {
            List<T> retval = new List<T>();
            int i = 0;
            while (channelReader.TryRead(out T item))
            {
                retval.Add(item);
                //TODO: Dont do this, but you know. Its kinda of how we got to do this atm
                if (++i > BufferSize)
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
                if (++i > BufferSize)
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