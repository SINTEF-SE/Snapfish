#define DEBUG
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;
using Snapfish.BL.Extensions;
using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;
using Snapfish.BL.Models.Logging;
using Snapfish.EkSeriesPubsubLibrary.Domain;

namespace Snapfish.EkSeriesPubsubLibrary
{
    /*
     * TODO: Implement cleanup AND CLEAN UP THE CODE. BECAUSE JEEEEEZ
     */
    public class EkSeriesSocketDaemon
    {
        private static readonly IPAddress Ek80Endpoint = IPAddress.Parse("10.218.68.70");
        private static readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SubscriptionReceiveEvent = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendQueueEmptied = new ManualResetEvent(false);
        private static readonly object SendLock = new object();
        private const int QueueSize = 256;
        private const int MaximumIncomingDatagrams = 1 << 8;

        private const int RemotePort = 37655;
        private const int ReceivePort = 8572;

        private object _responseObject;
        private EkSeriesServerInfo _remoteEkSeriesInfo;
        private static ConnectRequestReponseStruct _connectRequestResponseStruct;
        private static ConnectionToEkSeriesDevice _currentActiveConnection; //TODO: LIST?

        private static Dictionary<long, EkSeriesDataSubscriptionType> _subscriptionIdToTypeMap = new Dictionary<long, EkSeriesDataSubscriptionType>();

        /*
         * Max 256 concurrent 'connections' actually UDP datagrams incoming, because there is a bug in the socket implmentation in c# which makes the overlapped memory region un-freeable
         */
        private static StateObject[]
            _stateObjects = new StateObject[MaximumIncomingDatagrams];

        private static StateObject[] _subscriptionStateObjects = new StateObject[MaximumIncomingDatagrams];

        private static uint _previousSelectedStateObject = 0;
        private static uint _previousSelectedSubscriptionStateObjectIndex = 0;


        private static Boolean _isRetransmitting = false;
        public static bool _subscriptionMessageReceived = false;


        private static ConcurrentQueue<Ek80SendablePacketContainer> _sendQueue = new ConcurrentQueue<Ek80SendablePacketContainer>();
        private static readonly List<Ek80SendablePacketContainer> SentPackets = new List<Ek80SendablePacketContainer>();

        //TODO: Consider moving this into a container class which is stateful for each daemon. A daemon might in theory be connected to multiple EK devices. However, this will also require us to port currentActiveConnection to multiple conncetions

        #region API_VALUES 

        #region Parameters

        private static EkSeriesParameterType _currentEkSeriesParameterRequestType;
        private static string _applicationName;
        private static string _applicationType;
        private static string _applicationDescription;
        private static string _applicationVersion;
        private static string _channelID;
        private static string _frequency;
        private static string _pulseLength;
        private static string _sampleInterval;
        private static string _transmitPower;
        private static string _absorptionCoefficient;
        private static string _soundVelocity;
        private static string _transducerName;
        private static string _equivalentBeamAngle;
        private static string _angleSensitivityAlongship;
        private static string _angleSensitivityAthwartship;
        private static string _beamWidthAlongship;
        private static string _angleOffsetAlongship;
        private static string _gain;
        private static string _saCorrection;
        private static string _pingTime;
        private static string _latitude;
        private static string _longitude;
        private static string _heave;
        private static string _roll;
        private static string _pitch;
        private static string _distance;
        private static string _noiseEstimate;
        private static string _clientTimeoutLimit;

        //TODO: Make these configurable and storable

        #region SubscriptionParameters

        private EchogramConfiguration _echogramConfiguration = new EchogramConfiguration();

        #endregion

        #endregion

        #region Subscribables

        private Socket _subscriptionReceiver;
        private UdpClient _subscriptionReceiveClient;

        #endregion

        #endregion


        #region Logging

        private static readonly string SystemPath =
            Environment.GetFolderPath(Environment.SpecialFolder
                .CommonApplicationData); //So we dont go into permission problems

        private const string LogFilename = @"logfile";
        private static readonly string CompleteLogPath = Path.Combine(SystemPath, LogFilename);
        private const int LoggingMask = (int) LoggingFlags.All;
        private static SnapfishLogger _logger;
        private const string ApplicationLoggingPrefix = "Snapfish-logger:";

        #endregion
        
        /*
         * THIS IS TRASH! TODO: REMOVE ME AND STUFF FURUTHER DOWN WHEN WE KNOW HOW WE ARE GOING TO IMPLEMENT THIS
         */
        private static Channel<Echogram> _echogramSubscriptionQueue = null;
        private static Channel<SampleDataContainerClass> _sampleDataSubscriptionQueue = null;
        private static Channel<TargetsIntegration> _targetsIntegration = null;
        
        public EkSeriesSocketDaemon()
        {
            InitializeLogger();
            _logger.Info(
                "===================================|Booting up snapfish daemon|===================================");
            for (int i = 0; i < MaximumIncomingDatagrams; i++)
            {
                _stateObjects[i] = new StateObject();
                _subscriptionStateObjects[i] = new StateObject();
            }
        }

        #region Logging

        private void InitializeLogger()
        {
            InitializeLogFile();
            _logger = new SnapfishLogger(new StreamWriter(CompleteLogPath, true), (LoggingFlags) LoggingMask,
                ApplicationLoggingPrefix);
        }

        private static void InitializeLogFile() //Create if not exists, and append to if exists. Set Path regardless
        {
            if (!File.Exists(CompleteLogPath))
            {
                using (var fs = new FileStream(LogFilename, FileMode.Create))
                {
                }
            }
        }

        #endregion


        public void PublishSubscribable()
        {
            try
            {
                _logger.Info(
                    "----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})") /*, eventMessage.Id, Program.AppName, eventMessage)*/;
                //_eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.Throw(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}" /*, eventMessage.Id, Program.AppName*/);
                throw;
            }
        }

        public void HandshakeWithEkSeriesDevice()
        {
            IPEndPoint remoteEp = new IPEndPoint(Ek80Endpoint, RemotePort);
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            client.BeginConnect(remoteEp, ConnectCallback, client);
            ConnectDone.WaitOne();

            _logger.Info("Beginning send RSI:");
            Send(client, "RSI\0"); // Handshake to EK80
            SendDone.WaitOne();

            _logger.Info("Going to receive RSI");
            ReceiveSafeStruct<EkSeriesServerInfo>(client);
            ReceiveDone.WaitOne();

            _remoteEkSeriesInfo = (EkSeriesServerInfo) _responseObject;

            #region DEBUG

            Console.WriteLine("Completed handshake with: " + new string(_remoteEkSeriesInfo.ApplicationName) + " with description: " +
                              new string(_remoteEkSeriesInfo.ApplicationDescription));
            Console.WriteLine("The current ApplicationID is: " + _remoteEkSeriesInfo.ApplicationId + "\n" + "The remote processor unit has the following info + " +
                              "\n\tHostName: " + new string(_remoteEkSeriesInfo.HostName) + "\n\tCommandPort: " + _remoteEkSeriesInfo.CommandPort);
            _logger.Info("Completed handshake with: " + new string(_remoteEkSeriesInfo.ApplicationName) + " with description: " +
                         new string(_remoteEkSeriesInfo.ApplicationDescription));
            _logger.Info("The current ApplicationID is: " + _remoteEkSeriesInfo.ApplicationId + "\n" + "The remote processor unit has the following info + " +
                         "\n\tHostName: " + new string(_remoteEkSeriesInfo.HostName) + "\n\tCommandPort: " + _remoteEkSeriesInfo.CommandPort
            );

            #endregion

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }


        public void ConnectToRemoteEkDevice()
        {
            ConnectRequest request = new ConnectRequest {Header = "CON\0".ToCharArray(), ClientInfo = "Name:Simrad;Password:\0".ToCharArray()};
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remoteEndpoint = new IPEndPoint(Ek80Endpoint, (int) _remoteEkSeriesInfo.CommandPort);
            client.BeginConnect(remoteEndpoint, ConnectCallback, client);
            ConnectDone.WaitOne();

            Send(client, request);
            SendDone.WaitOne();

            ReceiveSafeStruct<Ek80Response>(client);
            ReceiveDone.WaitOne();

            Ek80Response response = (Ek80Response) _responseObject;
            _connectRequestResponseStruct = ParseResultsFromEkSeriesDevice(new string(response.MsgResponse));

            #region DEBUG // IFDEBUG?

            Console.WriteLine("Received a response to the connection request with the following info:\n" +
                              "\tResultCode: " + _connectRequestResponseStruct.ResultCode + "\n" +
                              "\tClientID: " + _connectRequestResponseStruct.ClientID + "\n" +
                              "\tAccessLevel: " + _connectRequestResponseStruct.AccessLevel + "\n");
            _logger.Debug("Received a response to the connection request with the following info:\n" +
                          "\tResultCode: " + _connectRequestResponseStruct.ResultCode + "\n" +
                          "\tClientID: " + _connectRequestResponseStruct.ClientID + "\n" +
                          "\tAccessLevel: " + _connectRequestResponseStruct.AccessLevel + "\n");

            #endregion

            _currentActiveConnection = new ConnectionToEkSeriesDevice {ActiveSocket = client, SequenceNumber = 1};
            Task.Factory.StartNew(() => BeginActiveOperationLoop(_currentActiveConnection.ActiveSocket),
                TaskCreationOptions.LongRunning);
            Thread sendThread = new Thread(SendThreadMethod) {IsBackground = true};

            IPEndPoint e = new IPEndPoint(IPAddress.Any, 8572);
            Socket subscriptionReceiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            subscriptionReceiver.Bind(e);
            sendThread.Start();
            Task.Factory.StartNew(() => SubscriptionSocketReceiver(subscriptionReceiver),
                TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private void SendPacketsFromQueueInOrder()
        {
            try
            {
                if (!_isRetransmitting)
                {
                    while (!_sendQueue.IsEmpty)
                    {

                        _sendQueue.TryPeek(out Ek80SendablePacketContainer structure);

                        Send(_currentActiveConnection.ActiveSocket, structure.SendableStruct.ToArray());
                        SendDone.WaitOne();
                        _logger.Info("Just sent a: " + structure.SendableStruct.GetName() + "   Packet" + " With SeqNo: " + structure.SendableStruct.GetSequenceNumber());
                        if (!_sendQueue.TryDequeue(out structure))
                        {
                            _logger.Alert("COULD NOT DEQUEUE AFTER SUCCESSFULL SEND QUEUE");
                        }
                    }
                }

                SendQueueEmptied.Set();
            }
            catch (Exception e)
            {
                _logger.Throw(e, "Send Packets from queue in order");
            }
        }

        private void BeginActiveOperationLoop(Socket activeSocket)
        {
            _logger.Info("Beginning active operation");
            while (true)
            {
                Receive(activeSocket);
                ReceiveDone.WaitOne();
            }
        }


        private void SubscriptionSocketReceiver(Socket receiveSocket)
        {
            while (true)
            {
                ReceiveSubscriptionData(receiveSocket);
                SubscriptionReceiveEvent.WaitOne();
            }
        }

        private void SendThreadMethod()
        {
            while (true)
            {
                SendPacketsFromQueueInOrder();
                SendQueueEmptied.WaitOne();
            }
        }

        private string CreateDataSubscribableMethodInvocationString(EkSeriesDataSubscriptionType subscriptionType)
        {
            string retval = "";
            switch (subscriptionType)
            {
                case EkSeriesDataSubscriptionType.BottomDetection:
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetection:
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp:
                    break;
                case EkSeriesDataSubscriptionType.SampleData:
                    retval = "SampleData," + "ChannelID=" + _channelID + ",SampleDataType=Sv,Range=100,RangeStart=10,";
                    break;
                case EkSeriesDataSubscriptionType.Echogram:
                    retval = "Echogram," + "PixelCount=" + _echogramConfiguration.PixelCount + "," + "ChannelID=" + _channelID + "," + "Range=" + _echogramConfiguration.Range +
                             "," + "RangeStart=" + _echogramConfiguration.RangeStart + ",";
                    break;
                case EkSeriesDataSubscriptionType.TargetsEchogram:
                    retval = "TargetsIntegration,ChannelID=" + _channelID +
                             ",State=Start,Layertype=Surface,Range=100,Rangestart=10,Margin=0.5,SvThreshold=-100.0,MinTSValue=-55.0,MinEcholength=0.7,MaxEcholength=2.0,MaxGainCompensation=6.0,MaxPhasedeviation=7.0";
                    break;
                case EkSeriesDataSubscriptionType.Integration:
                    break;
                case EkSeriesDataSubscriptionType.IntegrationChirp:
                    break;
                case EkSeriesDataSubscriptionType.TargetsIntegration:
                    break;
                case EkSeriesDataSubscriptionType.NoiseSpectrum:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionType), subscriptionType, null);
            }

            return retval;
        }

        public void SendSubscriptionRequest(EkSeriesRequestType requestType, EkSeriesDataSubscriptionType subscriptionType)
        {
            CommandRequest request = new CommandRequest {Header = "REQ\0".ToCharArray()};
            string method = "";
            string dataSubscriptionType = CreateDataSubscribableMethodInvocationString(subscriptionType);
            switch (requestType)
            {
                case EkSeriesRequestType.CreateDataSubscription:
                    method = "Subscribe";
                    break;
                case EkSeriesRequestType.ChangeDataSubscription:
                    method = "ChangeSubscription";
                    break;
                case EkSeriesRequestType.DestroyDataSubscription:
                    method = "Unsubscribe";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
            }

            #if DEBUG
            #region DEBUG_REGION
            
            switch (subscriptionType)
            {
                case EkSeriesDataSubscriptionType.Echogram:
                    Console.WriteLine("Sending echogram request with RID::" + _currentActiveConnection.SequenceNumber);
                    _logger.Info("Sending echogram request with RID::" + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.SampleData:
                    Console.WriteLine("SENDING SampleData REQUEST WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    _logger.Info("SENDING SampleData REQUEST WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.TargetsIntegration:
                    Console.WriteLine("SENDING BIOMASS REQUEST WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    _logger.Info("SENDING BIOMASS REQUEST WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.BottomDetection:
                    Console.WriteLine("SENDING Bottom Detection WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetection:
                    Console.WriteLine("SENDING TargetStrengthDetection WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp:
                    Console.WriteLine("SENDING TargetStrengthDetection Chirp WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.TargetsEchogram:
                    Console.WriteLine("SENDING TARGETS ECHOGRAM WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.Integration:
                    Console.WriteLine("SENDING INTEGRATION WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.IntegrationChirp:
                    Console.WriteLine("SENDING INTEGRATION CHIRP WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                case EkSeriesDataSubscriptionType.NoiseSpectrum:
                    Console.WriteLine("SENDING Noise Spectrum WITH RID:: " + _currentActiveConnection.SequenceNumber);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(subscriptionType), subscriptionType, null);
            }
            #endregion

            #endif
            
            request.MsgControl = (_currentActiveConnection.SequenceNumber + ",1,1\0\0\0\0").ToCharArray();
            request.MsgRequest = ("<request>" +
                                  "<clientInfo>" +
                                  "<cid>" + _connectRequestResponseStruct.ClientID + "</cid>" +
                                  "<rid>" + _currentActiveConnection.SequenceNumber + "</rid>" +
                                  "</clientInfo>" +
                                  "<type>invokeMethod</type>" +
                                  "<targetComponent>RemoteDataServer</targetComponent>" +
                                  "<method>" +
                                  "<" + method + ">" +
                                  "<requestedPort>" + ReceivePort.ToString() + "</requestedPort>" +
                                  "<dataRequest>" + dataSubscriptionType + "</dataRequest>" +
                                  "</" + method + ">" +
                                  "</method>" +
                                  "</request>"
                ).ToCharArray();
            request.SetRequestType("Subscription");
            request.SetMethodInvocationType(subscriptionType.ToString());
            EnqueuePacketToQueue(request, _currentActiveConnection.SequenceNumber++);
        }

        public void SendParameterRequestToEkSeriesDevice(EkSeriesParameterRequest ekSeriesParameterRequest, EkSeriesParameterType ekSeriesParameterType)
        {
            CommandRequest request = new CommandRequest
            {
                Header = "REQ\0".ToCharArray(),
                MsgControl = (_currentActiveConnection.SequenceNumber + ",1,1\0\0\0\0").ToCharArray(),
                MsgRequest = ("<request>" +
                              "<clientInfo>" +
                              "<cid>" + _connectRequestResponseStruct.ClientID + "</cid>" +
                              "<rid>" + _currentActiveConnection.SequenceNumber + "</rid>" +
                              "</clientInfo>" +
                              "<type>invokeMethod</type>" +
                              "<targetComponent>ParameterServer</targetComponent>" +
                              "<method>" +
                              "<GetParameter>" +
                              "<paramName>" + ekSeriesParameterType.GetParameterName() + "</paramName>" +
                              "<time>0</time>" +
                              "</GetParameter>" +
                              "</method>" +
                              "</request>\0").ToCharArray()
            };
            _currentEkSeriesParameterRequestType = ekSeriesParameterType;
            request.SetRequestType("Parameter");
            request.SetMethodInvocationType(ekSeriesParameterType.ToString());
            EnqueuePacketToQueue(request, _currentActiveConnection.SequenceNumber);
            _currentActiveConnection.SequenceNumber++;
        }

        private ConnectRequestReponseStruct ParseResultsFromEkSeriesDevice(string response)
        {
            ConnectRequestReponseStruct retval = new ConnectRequestReponseStruct {ResultCode = response.GetUntilOrEmpty(",").Replace("ResultCode:", "")};
            switch (retval.ResultCode)
            {
                case "S_OK":
                    break;
                case "E_ACCESSDENIED":
                    throw new Exception("ACCESS DENIED TO EK 80"); //TODO: PORT ME TO ERROR CODES
                case "E_FAIL":
                    throw new Exception("Operation failed due to an unspecified error");
                default:
                    _logger.Alert("Invalid result code from the connection request to EK80");
                    break;
            }

            string parameters = response.Substring(response.IndexOf(",", StringComparison.Ordinal) + 1);
            parameters = parameters.Between("{", "}");
            retval.ClientID = parameters.Between("ClientID:", ",");
            retval.AccessLevel = parameters.Between("AccessLevel:", "");
            return retval;
        }

        private static void RetransmitPackage(int sequenceNumber)
        {
            lock (SendLock)
            {
                _logger.Info("Entering retransmitPackage");
                _isRetransmitting = true;

                var packetToSend = SentPackets.Find(a => a.SequenceNumber == sequenceNumber);
                Send(_currentActiveConnection.ActiveSocket, packetToSend.SendableStruct.ToArray());
                SendDone.WaitOne();
                _logger.Info("Just retransmitted: " + packetToSend.SendableStruct.GetName() + "   Packet" + " With SeqNo: " + packetToSend.SendableStruct.GetSequenceNumber());
            }

            _isRetransmitting = false;
        }

        private static bool ValidateResponseMessageFromEkSeriesDevice(IEnumerable<XElement> errorcodes)
        {
            return !errorcodes.All(node => node.Value.Equals("0"));
        }

        private static void SubscriptionReceiverCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.WorkSocket;
                int receiveBytes = client.EndReceive(ar);
                if (state.Buffer.Length > 0)
                {
                    string header = Encoding.ASCII.GetString(state.Buffer, 0, 3);
                    switch (header)
                    {
                        case "REQ":
                            Console.Write("Received REQ IN SUB RECEIVER");
                            break;
                        case "ALI":
                            Console.Write("Received ALI IN SUB RECEIVER");
                            break;
                        case "RES":
                            Console.Write("Received RES IN SUB RECEIVER");
                            break;
                        case "PRD":
                            //TODO: NEXT STEP ON DRAGON BALL Z
                            Ek80ProcessedData processedData = new Ek80ProcessedData().FromArray(state.Buffer);
                            EkSeriesDataSubscriptionType type = _subscriptionIdToTypeMap[processedData.SubscriptionID];
                            switch (type)
                            {
                                case EkSeriesDataSubscriptionType.BottomDetection:
                                    break;
                                case EkSeriesDataSubscriptionType.TargetStrengthTsDetection:
                                    break;
                                case EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp:
                                    break;
                                case EkSeriesDataSubscriptionType.SampleData:
                                    SampleDataContainerClass sampleData = SampleDataContainerClass.FromArray(processedData.DataAsBytes);
                                    if (!_sampleDataSubscriptionQueue.Writer.TryWrite(sampleData))
                                    {
                                        Console.WriteLine("\"COULD NOT WRITE SampleData TO CHANNEL! W00t\"");
                                        _logger.Alert("COULD NOT WRITE SampleData TO CHANNEL! W00t");
                                    }

                                    break;
                                case EkSeriesDataSubscriptionType.Echogram:
                                    Echogram echogram = Echogram.FromArray(processedData.DataAsBytes);
                                    if (!_echogramSubscriptionQueue.Writer.TryWrite(echogram))
                                    {
                                        Console.WriteLine("\"COULD NOT WRITE ECHOGRAM TO CHANNEL! W00t\"");
                                        _logger.Alert("COULD NOT WRITE ECHOGRAM TO CHANNEL! W00t");
                                    }

                                    break;
                                case EkSeriesDataSubscriptionType.TargetsEchogram:
                                    TargetsIntegration integration = TargetsIntegration.FromArray(processedData.DataAsBytes);
                                    if (!_targetsIntegration.Writer.TryWrite(integration))
                                    {
                                        Console.WriteLine("\"COULD NOT WRITE TS TO CHANNEL! W00t\"");
                                        _logger.Alert("COULD NOT WRITE TS TO CHANNEL! W00t");
                                    }

                                    break;
                                case EkSeriesDataSubscriptionType.Integration:
                                    break;
                                case EkSeriesDataSubscriptionType.IntegrationChirp:
                                    break;
                                case EkSeriesDataSubscriptionType.TargetsIntegration:
                                    break;
                                case EkSeriesDataSubscriptionType.NoiseSpectrum:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case "RTR": //UNDOCUMENTED :: RETRANSMISSION PACKET
                            Console.Write("Received RTR IN SUB");
                            break;
                        default:
                            _logger.Throw(new NotImplementedException("MESSAGE TYPE NOT IMPLEMENTED:: " + header),
                                "damn");
                            break;
                    }
                }

                state.HasBeenProcessed = true;
                SubscriptionReceiveEvent.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine("SUP FOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOL");
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject) ar.AsyncState;
                Socket client = state.WorkSocket;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    string header = Encoding.ASCII.GetString(state.Buffer, 0, 3);
                    switch (header)
                    {
                        case "REQ":
                            break;
                        case "ALI":
                            AliveReport report = new AliveReport().FromArray(state.Buffer);
                            _logger.Info("Recieved ALI with the following data: " + new string(report.Info));
                            EnqueuePacketToQueue(AliveReport.GenerateAliveReport(_connectRequestResponseStruct.ClientID, _currentActiveConnection.SequenceNumber),
                                _currentActiveConnection.SequenceNumber);
                            break;
                        case "RES": // THIS WHOLE CODE IS A MESS, MIGRATE INTO METHOD AND DO ROBUST
                            //TODO: CHeck error  codes etc
                            Ek80Response response = new Ek80Response().FromArray(state.Buffer);
                            XElement messageResponse = XElement.Parse(response.GetResponse().Trim('\0'));

                            XElement faultNode = messageResponse.Elements().FirstOrDefault(e => e.Name.LocalName == "fault");
                            IEnumerable<XElement> errorcodes = faultNode?.Elements().FirstOrDefault(e => e.Name.LocalName == "detail")
                                ?.Elements().Where(e => e.Name.LocalName != "message");
                            bool containsErrors = ValidateResponseMessageFromEkSeriesDevice(errorcodes);
                            //TODO: IMPLEMENT callback here and push a RequestFailtureStruct
                            if (containsErrors)
                            {
                                _logger.Critical("Invalid parameter request. I have received error codes");
                                break;
                            }


                            XElement responseSubtree = null;
                            bool exceptionTriggered = false;
                            try
                            {
                                responseSubtree = XElement.Parse(faultNode?.NextNode.ToString());
                            }
                            catch (Exception e)
                            {
                                exceptionTriggered = true;
                            }

                            //EKSeries does not guarantee order. This is crazy so we need this. I am sorry! There is a better way but I want to do it 'generic' due to time constrains
                            if (exceptionTriggered)
                            {
                                responseSubtree = XElement.Parse(faultNode?.PreviousNode.ToString());
                            }


                            switch (responseSubtree.Name.ToString())
                            {
                                case "GetParameterResponse":
                                    XElement paramValue = responseSubtree.Elements()
                                        .FirstOrDefault(e => e.Name.LocalName == "paramValue");

                                    foreach (var packet in SentPackets)
                                    {
                                        if (packet.SequenceNumber == Int64.Parse(((XElement) ((XElement) messageResponse.FirstNode).LastNode).Value))
                                        {
                                            if (packet.SendableStruct.GetRequestType() == "Parameter")
                                            {
                                                // ALL good
                                                switch (packet.SendableStruct.GetMethodInvocationType())
                                                {
                                                    case nameof(EkSeriesParameterType.GetApplicationName):
                                                        _applicationName = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetApplicationType):
                                                        _applicationType = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetApplicationDescription):
                                                        _applicationDescription = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetApplicationVersion):
                                                        _applicationVersion = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetChannelId):
                                                        _channelID = paramValue?.Elements().FirstOrDefault(e => e.Name.LocalName == "value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetFrequency):
                                                        _frequency = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetPulseLength):
                                                        _pulseLength = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetSampleInterval):
                                                        _sampleInterval = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.GetTransmitPower):
                                                        _transmitPower = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.AbsorptionCoefficient):
                                                        _absorptionCoefficient = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.SoundVelocity):
                                                        _soundVelocity = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.TransducerName):
                                                        _transducerName = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.EquivalentBeamAngle):
                                                        _equivalentBeamAngle = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.AngleSensitivityAlongship):
                                                        _angleSensitivityAlongship = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.AngleSensitivityAthwartship):
                                                        _angleSensitivityAthwartship = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.BeamWidthAlongship):
                                                        _beamWidthAlongship = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.AngleOffsetAlongship):
                                                        _angleOffsetAlongship = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Gain):
                                                        _gain = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.SaCorrection):
                                                        _saCorrection = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.PingTime):
                                                        _pingTime = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Latitude):
                                                        _latitude = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Longitude):
                                                        _longitude = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Heave):
                                                        _heave = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Roll):
                                                        _roll = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Pitch):
                                                        _pitch = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.Distance):
                                                        _distance = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.NoiseEstimate):
                                                        _noiseEstimate = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.ClientTimeoutLimit):
                                                        _clientTimeoutLimit = paramValue?.Element("value")?.Value;
                                                        break;
                                                    case nameof(EkSeriesParameterType.ApplicationType):
                                                        _applicationType = paramValue?.Element("value")?.Value;
                                                        break;
                                                    default:
                                                        throw new ArgumentOutOfRangeException();
                                                }
                                            }
                                        }
                                    }

                                    break;
                                case "SubscribeResponse":
                                    //PARSE SUBSCRIPTION ID
                                    var subscriptionNode = responseSubtree.Elements().FirstOrDefault(e => e.Name.LocalName == "subscriptionID");
                                    Console.WriteLine("Receive a subscribe reponse with id: " + subscriptionNode.ToString());
                                    if (_subscriptionIdToTypeMap.ContainsKey(Int64.Parse(subscriptionNode.Value)))
                                    {
                                        break;
                                    }

                                    foreach (var packet in SentPackets.ToList()) //Need a copy of the list to avoid enumerating while the collection is being modified.
                                    {
                                        if (packet.SequenceNumber == Int64.Parse(((XElement) ((XElement) messageResponse.FirstNode).LastNode).Value))
                                        {
                                            if (packet.SendableStruct.GetRequestType() == "Subscription")
                                            {
                                                EkSeriesDataSubscriptionType type;
                                                switch (packet.SendableStruct.GetMethodInvocationType())
                                                {
                                                    case nameof(EkSeriesDataSubscriptionType.BottomDetection):
                                                        type = EkSeriesDataSubscriptionType.BottomDetection;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.TargetStrengthTsDetection):
                                                        type = EkSeriesDataSubscriptionType.TargetStrengthTsDetection;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp):
                                                        type = EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.SampleData):
                                                        type = EkSeriesDataSubscriptionType.SampleData;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.Echogram):
                                                        type = EkSeriesDataSubscriptionType.Echogram;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.TargetsEchogram):
                                                        type = EkSeriesDataSubscriptionType.TargetsEchogram;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.Integration):
                                                        type = EkSeriesDataSubscriptionType.Integration;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.IntegrationChirp):
                                                        type = EkSeriesDataSubscriptionType.IntegrationChirp;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.TargetsIntegration):
                                                        type = EkSeriesDataSubscriptionType.TargetsIntegration;
                                                        break;
                                                    case nameof(EkSeriesDataSubscriptionType.NoiseSpectrum):
                                                        type = EkSeriesDataSubscriptionType.NoiseSpectrum;
                                                        break;
                                                    default:
                                                        throw new ArgumentOutOfRangeException();
                                                }

                                                _subscriptionIdToTypeMap.Add(Int64.Parse(subscriptionNode.Value), type);
                                            }
                                        }
                                    }

                                    break;
                            }

                            break;
                        case "PRD":
                            Ek80ProcessedData processedData = new Ek80ProcessedData().FromArray(state.Buffer);
                            _logger.Info("Parsing processed data");
                            break;
                        case "RTR": //UNDOCUMENTED :: RETRANSMISSION PACKET
                            //Convert to ascii
                            //RTR1,0,0
                            string rtrData = Encoding.ASCII.GetString(state.Buffer);
                            string rtrSeqNo = rtrData.GetUntilOrEmpty(",");
                            rtrSeqNo = rtrSeqNo.Remove(0, 4);
                            RetransmitPackage(Int32.Parse(rtrSeqNo));
                            _logger.Info("EKSeriesDeviceRequestingRetransmission of packet: " + rtrSeqNo);
                            break;
                        default:
                            _logger.Throw(new NotImplementedException("MESSAGE TYPE NOT IMPLEMENTED:: " + header),
                                "damn");
                            break;
                    }
                }

                state.HasBeenProcessed = true;
                ReceiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public EchogramTransmissionPacket GetEchogramTransmissionPacket(int limit)
        {
            EchogramTransmissionPacket retval = new EchogramTransmissionPacket();

            
            
            return retval;
        }
        

        #region PARAMETER_GETTERS

        public string ApplicationName => _applicationName;

        public string ApplicationType => _applicationType;

        public string ApplicationDescription => _applicationDescription;

        public string ApplicationVersion => _applicationVersion;

        public string ChannelId => _channelID;

        public string Frequency => _frequency;

        public string PulseLength => _pulseLength;

        public string SampleInterval => _sampleInterval;

        public string TransmitPower => _transmitPower;

        public string AbsorptionCoefficient => _absorptionCoefficient;

        public string SoundVelocity => _soundVelocity;

        public string TransducerName => _transducerName;

        public string EquivalentBeamAngle => _equivalentBeamAngle;

        public string AngleSensitivityAlongship => _angleSensitivityAlongship;

        public string AngleSensitivityAthwartship => _angleSensitivityAthwartship;

        public string BeamWidthAlongship => _beamWidthAlongship;

        public string AngleOffsetAlongship => _angleOffsetAlongship;

        public string Gain => _gain;

        public string SaCorrection => _saCorrection;

        public string PingTime => _pingTime;

        public string Latitude => _latitude;

        public string Longitude => _longitude;

        public string Heave => _heave;

        public string Roll => _roll;

        public string Pitch => _pitch;

        public string Distance => _distance;

        public string NoiseEstimate => _noiseEstimate;

        public string ClientTimeoutLimit => _clientTimeoutLimit;

        #endregion

        #region PACKET MANAGEMENT

        //TODO: REMOVE ME, NOT NEEDED
        private static void EnqueuePacketToQueue(ISendableStruct package, int SequenceNumber)
        {
            Ek80SendablePacketContainer container = new Ek80SendablePacketContainer {SendableStruct = package, SequenceNumber = SequenceNumber};
            if (_sendQueue.Count >= QueueSize)
            {
                for (int i = 0; i < QueueSize / 2; i++)
                {
                    Ek80SendablePacketContainer tmp;
                    _sendQueue.TryDequeue(out tmp);
                }
            }

            _sendQueue.Enqueue(container);
            EnqueueToOldPacketList(package, SequenceNumber);
        }

        private static void EnqueueToOldPacketList(ISendableStruct package, int sequenceNumber)
        {
            Ek80SendablePacketContainer container = new Ek80SendablePacketContainer {SendableStruct = package, SequenceNumber = sequenceNumber};
            if (SentPackets.Count >= QueueSize)
            {
                for (int i = 0; i < QueueSize / 2; i++)
                {
                    SentPackets.RemoveAt(0);
                }
            }

            SentPackets.Add(container);
        }

        #endregion

        #region GENERAL_PURPOSE_ASYNC_SOCKET_FUNCTIONALITY

        private static void ReceiveSubscriptionData(Socket server)
        {
            try
            {
                // Create the state object.
                StateObject state = GetAvailableSubscriptionStateObject();
                state.WorkSocket = server;
                state.HasBeenProcessed = false;
                //Console.WriteLine("Receiving subscription data from: " + state.WorkSocket.RemoteEndPoint.ToString());
                server.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(SubscriptionReceiverCallback), state);
            }
            catch (Exception e)
            {
                _logger.Throw(e, "Exception occured during ReceiveSubscriptionData: ");
            }
        }

        private static StateObject GetAvailableSubscriptionStateObject()
        {
            try
            {
                StateObject retval = _subscriptionStateObjects[++_previousSelectedSubscriptionStateObjectIndex % MaximumIncomingDatagrams];
                while (!retval.HasBeenProcessed)
                {
                    retval = _subscriptionStateObjects[++_previousSelectedSubscriptionStateObjectIndex % MaximumIncomingDatagrams];
                }

                return retval;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        //Just round robin until we find state object
        private static StateObject GetAvailableStateObject()
        {
            try
            {
                StateObject retval = _stateObjects[++_previousSelectedStateObject % MaximumIncomingDatagrams];
                while (!retval.HasBeenProcessed)
                {
                    retval = _stateObjects[++_previousSelectedStateObject % MaximumIncomingDatagrams];
                }

                return retval;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }

        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.
                StateObject state = GetAvailableStateObject();
                state.WorkSocket = client;
                state.HasBeenProcessed = false;
                //StateObject state = new StateObject {WorkSocket = client, HasBeenProcessed = false};
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReceiveCallback, state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Send<T>(Socket client, T structure) where T : struct, ISendableStruct
        {
            byte[] data = structure.ToArray();
            _logger.Info("SENDING: " + Encoding.ASCII.GetString(data));
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void Send(Socket client, byte[] data)
        {
            _logger.Info("SENDING: " + Encoding.ASCII.GetString(data));
            client.BeginSend(data, 0, data.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private void Send(Socket client, string data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            _logger.Info("SENDING: " + Encoding.ASCII.GetString(byteData));
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                _logger.Info("Sent " + bytesSent + "bytes to server at: " + client.RemoteEndPoint.ToString());
                SendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveSafeStructCallback<T>(IAsyncResult ar) where T : struct, IConvertable<T>
        {
            try
            {
                _logger.Info("Inside ReceiveSafeStructCallback");
                StructureStateObject<T> state = (StructureStateObject<T>) ar.AsyncState;
                Socket client = state.WorkSocket;
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // FIGURE OUT AT SOME POINT IF WE NEED TO DO SOME BYTES READ VS SIZE POPULATING BUFFERS FOR MARSHALLING
                    var responseStruct = new T();
                    state.Structure = responseStruct.FromArray(state.buffer);
                    _responseObject = state.Structure;
                    ReceiveDone.Set();
                }
            }
            catch (Exception e)
            {
                _logger.Alert(e.ToString());
            }
        }

        public void CreateEchogramSubscription(ref Channel<Echogram> echogramSubscriptionQueue)
        {
            _echogramSubscriptionQueue = echogramSubscriptionQueue;
            SendSubscriptionRequest(EkSeriesRequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.Echogram);
        }

        public void CreateSampleDataSubscription(ref Channel<SampleDataContainerClass> sampleDataSubscriptionQueue)
        {
            _sampleDataSubscriptionQueue = sampleDataSubscriptionQueue;
            SendSubscriptionRequest(EkSeriesRequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.SampleData);
        }

        public void CreateBiomassSubscription(ref Channel<TargetsIntegration> targetsIntegrationQueue)
        {
            _targetsIntegration = targetsIntegrationQueue;
            SendSubscriptionRequest(EkSeriesRequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.TargetsIntegration);
        }

        //_sampleDataSubscriptionQueue

        private void ReceiveSafeStruct<T>(Socket client) where T : struct, IConvertable<T>
        {
            try
            {
                _logger.Info("Entering Safe Receive Struct");
                StructureStateObject<T> state = new StructureStateObject<T> {WorkSocket = client};
                client.BeginReceive(state.buffer, 0, StructureStateObject<T>.BufferSize, 0,
                    new AsyncCallback(ReceiveSafeStructCallback<T>), state);
            }
            catch (Exception e)
            {
                _logger.Alert(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket) ar.AsyncState;
                client.EndConnect(ar);
                ConnectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion
    }
}