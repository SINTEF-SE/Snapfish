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
using Snapfish.EkSeriesPubsubLibrary.Domain;

namespace Snapfish.EkSeriesPubsubLibrary
{
    /*
     * TODO: Implement cleanup AND CLEAN UP THE CODE. BECAUSE JEEEEEZ
     */
    public class EkSeriesSocketDaemon
    {
        private struct UdpState
        {
            public UdpClient u;
            public IPEndPoint e;
        }
        
        private static readonly IPAddress Ek80Endpoint = IPAddress.Parse("10.218.68.70");
        private static readonly ManualResetEvent ConnectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent ReceiveDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent SubscriptionReceiveEvent = new ManualResetEvent(false);
        private static readonly ManualResetEvent SendQueueEmptied = new ManualResetEvent(false);
        private static readonly object SendLock = new object();
        private static readonly int QueueSize = 256;

        private const int RemotePort = 37655;
        private const int _receivePort = 8572;

        private Object _responseObject;
        private ServerInfo _remoteEkSeriesInfo;
        private static ConnectRequestReponseStruct _connectRequestReponseStruct;
        private static ConnectionToEkSeriesDevice _currentActiveConnection; //TODO: LIST?
        private static Boolean isRetransmitting = false;
        private static bool _subscriptionMessageReceived = false;

        private static ConcurrentQueue<Ek80SendablePacketContainer> _sendQueue = new ConcurrentQueue<Ek80SendablePacketContainer>();
        private static readonly List<Ek80SendablePacketContainer> SentPackets = new List<Ek80SendablePacketContainer>();

        //TODO: Consider moving thisa into a container class which is stateful for each daemon. A daemon might in theory be connected to multiple EK devices

        #region API_VALUES 

        #region Parameters

        private static ParameterType _currentParameterRequestType;
        private static string _applicationName;
        private static string _channelID;

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

        // This might be moved/removed
        //private readonly IEventBus _eventBus = null;
        
        
        /*
         * THIS IS TRASH! TODO: REMOVE ME AND STUFF FURUTHER DOWN WHEN WE KNOW HOW WE ARE GOING TO IMPLEMENT THIS
         */
        private static Channel<Echogram> _echogramSubscriptionQueue = null;

        public EkSeriesSocketDaemon()
        {
            InitializeLogger();
            _logger.Info(
                "===================================|Booting up snapfish daemon|===================================");
        }
        
    /*   public EkSeriesSocketDaemon(IEventBus eventBus)
        {
            _eventBus = eventBus;
            InitializeLogger();
            _logger.Info(
                "===================================|Booting up snapfish daemon|===================================");
        } */

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
                _logger.Info("----- Publishing integration event: {IntegrationEventId} from {AppName} - ({@IntegrationEvent})")/*, eventMessage.Id, Program.AppName, eventMessage)*/;
                //_eventBus.Publish(eventMessage);
            }
            catch (Exception ex)
            {
                _logger.Throw(ex, "ERROR Publishing integration event: {IntegrationEventId} from {AppName}"/*, eventMessage.Id, Program.AppName*/);
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
            ReceiveSafeStruct<ServerInfo>(client);
            ReceiveDone.WaitOne();

            _remoteEkSeriesInfo = (ServerInfo) _responseObject;

            #region DEBUG

            Console.WriteLine("Completed handshake with: " + new string(_remoteEkSeriesInfo.ApplicationName) + " with description: " +
                              new string(_remoteEkSeriesInfo.ApplicationDescription));
            Console.WriteLine("The current ApplicationID is: " + _remoteEkSeriesInfo.ApplicationID + "\n" + "The remote processor unit has the following info + " +
                              "\n\tHostName: " + new string(_remoteEkSeriesInfo.HostName) + "\n\tCommandPort: " + _remoteEkSeriesInfo.CommandPort);
            _logger.Info("Completed handshake with: " + new string(_remoteEkSeriesInfo.ApplicationName) + " with description: " +
                         new string(_remoteEkSeriesInfo.ApplicationDescription));
            _logger.Info("The current ApplicationID is: " + _remoteEkSeriesInfo.ApplicationID + "\n" + "The remote processor unit has the following info + " +
                         "\n\tHostName: " + new string(_remoteEkSeriesInfo.HostName) + "\n\tCommandPort: " + _remoteEkSeriesInfo.CommandPort
            );

            #endregion

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

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

        public void ConnectToRemoteEkDevice()
        {
            ConnectRequest request = new ConnectRequest {Header = "CON\0".ToCharArray(), ClientInfo = "Name:Simrad;Password:\0".ToCharArray()};
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint remoteEndpoint = new IPEndPoint(Ek80Endpoint, (int) _remoteEkSeriesInfo.CommandPort);
            client.BeginConnect(remoteEndpoint, ConnectCallback, client);
            ConnectDone.WaitOne();

            Send(client, request);
            SendDone.WaitOne();

            ReceiveSafeStruct<EK80Response>(client);
            ReceiveDone.WaitOne();

            EK80Response response = (EK80Response) _responseObject;
            _connectRequestReponseStruct = ParseResultsFromEkSeriesDevice(new string(response.MsgResponse));

            #region DEBUG // IFDEBUG?

            Console.WriteLine("Received a response to the connection request with the following info:\n" +
                              "\tResultCode: " + _connectRequestReponseStruct.ResultCode + "\n" +
                              "\tClientID: " + _connectRequestReponseStruct.ClientID + "\n" +
                              "\tAccessLevel: " + _connectRequestReponseStruct.AccessLevel + "\n");
            _logger.Debug("Received a response to the connection request with the following info:\n" +
                          "\tResultCode: " + _connectRequestReponseStruct.ResultCode + "\n" +
                          "\tClientID: " + _connectRequestReponseStruct.ClientID + "\n" +
                          "\tAccessLevel: " + _connectRequestReponseStruct.AccessLevel + "\n");

            #endregion

            _currentActiveConnection = new ConnectionToEkSeriesDevice {ActiveSocket = client, SequenceNumber = 1};
            Task.Factory.StartNew(() => BeginActiveOperationLoop(_currentActiveConnection.ActiveSocket),
                TaskCreationOptions.LongRunning);
            Thread sendThread = new Thread(new ThreadStart(SendThreadMethod)) {IsBackground = true};
            Thread receiveThread = new Thread(new ThreadStart(SubscriptionReceiver)) {IsBackground = true};
            sendThread.Start();
            receiveThread.Start();
        }

        //TODO: Use size for RTR on 256, pop 128
        private void SendPacketsFromQueueInOrder()
        {
            try
            {
                if (!isRetransmitting)
                {
                    while (!_sendQueue.IsEmpty)
                    {
                        Ek80SendablePacketContainer structure;
                        _sendQueue.TryPeek(out structure);
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
        
        
        private void SubscriptionReceiver()
        {
            IPEndPoint e = new IPEndPoint(IPAddress.Any, 8572);
            UdpClient u = new UdpClient(e);
            UdpState s = new UdpState {e = e, u = u};
            //_subscriptionMessageReceived
            Console.WriteLine("Listening for subscription messages");
            while (true)
            {
                //ReceiveSubscriptionData(u, e);
                u.BeginReceive(SubscriptionReceiverCallback, s);
                SubscriptionReceiveEvent.WaitOne();
            }
            // ReSharper disable once FunctionNeverReturns
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
                    break;
                case EkSeriesDataSubscriptionType.Echogram:
                    retval = "Echogram," + "PixelCount=" + _echogramConfiguration.PixelCount + "," + "ChannelID=" + _channelID + "," + "Range=" + _echogramConfiguration.Range +
                             "," + "RangeStart=" + _echogramConfiguration.RangeStart + ",";
                    break;
                case EkSeriesDataSubscriptionType.TargetsEchogram:
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

        public void SendSubscriptionRequest(Ek80RequestType requestType, EkSeriesDataSubscriptionType subscriptionType)
        {
            CommandRequest request = new CommandRequest {Header = "REQ\0".ToCharArray()};
            string method = "";
            string dataSubscriptionType = CreateDataSubscribableMethodInvocationString(subscriptionType);
            switch (requestType)
            {
                case Ek80RequestType.CreateDataSubscription:
                    method = "Subscribe";
                    break;
                case Ek80RequestType.ChangeDataSubscription:
                    method = "ChangeSubscription";
                    break;
                case Ek80RequestType.DestroyDataSubscription:
                    method = "Unsubscribe";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
            }

            request.MsgControl = (_currentActiveConnection.SequenceNumber + ",1,1\0\0\0\0").ToCharArray();
            request.MsgRequest = ("<request>" +
                                  "<clientInfo>" +
                                  "<cid>" + _connectRequestReponseStruct.ClientID + "</cid>" +
                                  "<rid>" + _currentActiveConnection.SequenceNumber + "</rid>" +
                                  "</clientInfo>" +
                                  "<type>invokeMethod</type>" +
                                  "<targetComponent>RemoteDataServer</targetComponent>" +
                                  "<method>" +
                                  "<" + method + ">" +
                                  "<requestedPort>" + _receivePort.ToString() + "</requestedPort>" +
                                  "<dataRequest>" + dataSubscriptionType + "</dataRequest>" +
                                  "</" + method + ">" +
                                  "</method>" +
                                  "</request>"
                ).ToCharArray();
            EnqueuePacketToQueue(request, _currentActiveConnection.SequenceNumber++);
        }

        public void SendParameterRequestToEkSeriesDevice(ParameterRequestType parameterRequest, ParameterType parameterType)
        {
            CommandRequest request = new CommandRequest
            {
                Header = "REQ\0".ToCharArray(),
                MsgControl = (_currentActiveConnection.SequenceNumber + ",1,1\0\0\0\0").ToCharArray(),
                MsgRequest = ("<request>" +
                              "<clientInfo>" +
                              "<cid>" + _connectRequestReponseStruct.ClientID + "</cid>" +
                              "<rid>" + _currentActiveConnection.SequenceNumber + "</rid>" +
                              "</clientInfo>" +
                              "<type>invokeMethod</type>" +
                              "<targetComponent>ParameterServer</targetComponent>" +
                              "<method>" +
                              "<GetParameter>" +
                              "<paramName>" + parameterType.GetParameterName() + "</paramName>" +
                              "<time>0</time>" +
                              "</GetParameter>" +
                              "</method>" +
                              "</request>\0").ToCharArray()
            };
            _currentParameterRequestType = parameterType;
            EnqueuePacketToQueue(request, _currentActiveConnection.SequenceNumber);
            _currentActiveConnection.SequenceNumber++;
        }

        private ConnectRequestReponseStruct ParseResultsFromEkSeriesDevice(string response)
        {
            ConnectRequestReponseStruct retval = new ConnectRequestReponseStruct();
            retval.ResultCode = response.GetUntilOrEmpty(",").Replace("ResultCode:", "");
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

        // TODO: Move me into the struct and create aux methods on the actual struct once shit....works
        private static AliveReport GenerateAliveReport(int sequenceNumber)
        {
            return new AliveReport
            {
                Header = "ALI\0".ToCharArray(),
                Info = ("ClientID:" + _connectRequestReponseStruct.ClientID + ",SeqNo:" +
                        sequenceNumber + "\0").ToCharArray()
            };
        }

        private static void RetransmitPackage(int sequenceNumber)
        {
            lock (SendLock)
            {
                //retransmit
                _logger.Info("Entering retransmitPackage");
                isRetransmitting = true;

                var packetToSend = SentPackets.Find(a => a.SequenceNumber == sequenceNumber);
                Send(_currentActiveConnection.ActiveSocket, packetToSend.SendableStruct.ToArray());
                SendDone.WaitOne();
                _logger.Info("Just retransmitted: " + packetToSend.SendableStruct.GetName() + "   Packet" + " With SeqNo: " + packetToSend.SendableStruct.GetSequenceNumber());
            }

            isRetransmitting = false;
        }

        private static bool ValidateResponseMessageFromEkSeriesDevice(IEnumerable<XElement> errorcodes)
        {
            return !errorcodes.All(node => node.Value.Equals("0"));
        }

        private static void SubscriptionReceiverCallback(IAsyncResult ar)
        {
            try
            {
                Console.WriteLine("HERE WE GO! ");
                UdpClient u = ((UdpState)(ar.AsyncState)).u;
                IPEndPoint e = ((UdpState)(ar.AsyncState)).e;
                byte[] receiveBytes = u.EndReceive(ar, ref e);
                if (receiveBytes.Length > 0)
                {
                    string header = Encoding.ASCII.GetString(receiveBytes, 0, 3);
                    switch(header)
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
                            Ek80ProcessedData processedData = new Ek80ProcessedData().FromArray(receiveBytes);
                            byte[] intermediateDataFormat = new byte[processedData.Data.Length * 2];
                            Buffer.BlockCopy(processedData.Data, 0, intermediateDataFormat, 0, (processedData.Data.Length * 2));
                            Echogram echogram = Echogram.FromArray(intermediateDataFormat);
                            //_echogramSubscriptionQueue.Enqueue(echogram);
                            if (!_echogramSubscriptionQueue.Writer.TryWrite(echogram))
                            {
                                _logger.Alert("COULD NOT WRITE ECHOGRAM TO CHANNEL! W00t");
                            }
                            Console.WriteLine("SEXY");
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
                SubscriptionReceiveEvent.Set();
            }
            catch (Exception e)
            {
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
                            EnqueuePacketToQueue(GenerateAliveReport(_currentActiveConnection.SequenceNumber), _currentActiveConnection.SequenceNumber);
                            break;
                        case "RES":
                            //TODO: CHeck error  codes etc
                            EK80Response response = new EK80Response().FromArray(state.Buffer);
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

                            XElement responseSubtree = XElement.Parse(faultNode?.NextNode.ToString());
                            switch (responseSubtree.Name.ToString())
                            {
                                case "GetParameterResponse":
                                    XElement paramValue = responseSubtree.Elements()
                                        .FirstOrDefault(e => e.Name.LocalName == "paramValue");

                                    // PARAMETER REQUEST
                                    switch (_currentParameterRequestType)
                                    {
                                        case ParameterType.GetApplicationName:
                                            _applicationName = paramValue?.Element("value")?.Value;
                                            break;
                                        case ParameterType.GetChannelId:
                                            Console.WriteLine(paramValue?.Value);
                                            _channelID = paramValue?.Value;
                                            break;
                                    }

                                    string value = paramValue?.Element("value")?.Value;
                                    string time = paramValue?.Element("time")?.Value;
                                    Console.WriteLine(value + " :::: " + time);
                                    break;
                                case "SubscribeResponse":
                                    Console.WriteLine("COOL STUFF");
                                    /*
                                     * <SubscribeResponse>
                                          <subscriptionID dt="3">2</subscriptionID>
                                        </SubscribeResponse>
                                     */
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

                ReceiveDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #region GENERAL_PURPOSE_ASYNC_SOCKET_FUNCTIONALITY
        
        private static void ReceiveSubscriptionData(UdpClient client, IPEndPoint endpoint)
        {
            try
            {
                // Create the state object.
                UdpState state = new UdpState() {u = client, e = endpoint};
                var byteFace = client.Receive(ref state.e);
                Console.WriteLine(byteFace);
            }
            catch (Exception e)
            {
                _logger.Throw(e, "Exception occured during ReceiveSubscriptionData: ");
            } 
        }
        
        private static void ReceiveSubscriptionData(Socket server)
        {
           
            try
            {
                // Create the state object.
                StateObject state = new StateObject {WorkSocket = server};
                //Console.WriteLine("Receiving subscription data from: " + state.WorkSocket.RemoteEndPoint.ToString());
                server.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(SubscriptionReceiverCallback), state);
            }
            catch (Exception e)
            {
                _logger.Throw(e, "Exception occured during ReceiveSubscriptionData: ");
            } 
        }
        
        private static void Receive(Socket client)
        {
            try
            {
                // Create the state object.  
                StateObject state = new StateObject {WorkSocket = client};
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
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
            SendSubscriptionRequest(Ek80RequestType.CreateDataSubscription, EkSeriesDataSubscriptionType.Echogram);
        }

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