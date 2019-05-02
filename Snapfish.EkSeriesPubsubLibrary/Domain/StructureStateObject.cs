using System.Net.Sockets;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class StructureStateObject<T>
    {
        public Socket WorkSocket = null;
        public const int BufferSize = 1 << 12;
        public byte[] buffer = new byte[BufferSize];
        public T Structure = default(T);
    }
}