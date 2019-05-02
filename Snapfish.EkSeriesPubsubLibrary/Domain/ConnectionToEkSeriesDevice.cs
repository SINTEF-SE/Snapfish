using System.Net.Sockets;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class ConnectionToEkSeriesDevice
    {
        public Socket ActiveSocket = null;
        public const int BufferSize = 1 << 12;
        public byte[] Buffer = new byte[BufferSize];
        public int SequenceNumber = 1;
    }
}