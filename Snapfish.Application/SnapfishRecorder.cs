using System.Threading.Channels;

namespace Snapfish.Application
{
    public class SnapfishRecorder
    {
        public static Channel<byte[]> BoundedBuffer;
    }
}