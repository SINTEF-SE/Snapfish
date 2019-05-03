using System.ComponentModel;
using System.Threading.Channels;

namespace Snapfish.Application
{
    public class SnapfishRecorder
    {
        private const int _bufferSize = 1 << 12;  
        private static Channel<byte[]> _boundedBuffer;


        public SnapfishRecorder()
        {
            _boundedBuffer = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(_bufferSize)
            {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleWriter = true,
                SingleReader = false
            });
            
            
            
        }
    }
}