using System.IO;

namespace Snapfish.BL.Models
{
    public struct ServerInfo : IConvertable<ServerInfo>
    {
        public char[] Header;
        public char[] ApplicationType;
        public char[] ApplicationName;
        public char[] ApplicationDescription;
        public long ApplicationID;
        public long CommandPort;
        public long Mode;
        public char[] HostName;
        
        public ServerInfo FromArray(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            var s = default(ServerInfo);
            s.Header = reader.ReadChars(4);
            s.ApplicationType = reader.ReadChars(64);
            s.ApplicationName = reader.ReadChars(64);
            s.ApplicationDescription = reader.ReadChars(128);
            s.ApplicationID = reader.ReadInt32();
            s.CommandPort = reader.ReadInt32();
            s.Mode = reader.ReadInt32();
            s.HostName = reader.ReadChars(64);
            return s;
        }
    }
}