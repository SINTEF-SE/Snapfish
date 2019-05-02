using System.Net.Sockets;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class StateObject
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 1 << 12;
        public readonly byte[] Buffer = new byte[BufferSize];
    }
}