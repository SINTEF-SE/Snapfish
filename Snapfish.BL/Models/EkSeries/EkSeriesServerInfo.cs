using System.IO;

namespace Snapfish.BL.Models.EkSeries
{
    public struct EkSeriesServerInfo : IConvertable<EkSeriesServerInfo>
    {
        public char[] Header;
        public char[] ApplicationType;
        public char[] ApplicationName;
        public char[] ApplicationDescription;
        public long ApplicationId;
        public long CommandPort;
        public long Mode;
        public char[] HostName;
        
        public EkSeriesServerInfo FromArray(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            var s = default(EkSeriesServerInfo);
            s.Header = reader.ReadChars(4);
            s.ApplicationType = reader.ReadChars(64);
            s.ApplicationName = reader.ReadChars(64);
            s.ApplicationDescription = reader.ReadChars(128);
            s.ApplicationId = reader.ReadInt32();
            s.CommandPort = reader.ReadInt32();
            s.Mode = reader.ReadInt32();
            s.HostName = reader.ReadChars(64);
            return s;
        }
    }
}