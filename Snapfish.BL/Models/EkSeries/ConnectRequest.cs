using System.IO;

namespace Snapfish.BL.Models.EkSeries
{
    public struct ConnectRequest : ISendableStruct
    {
        public char[] Header; //4
        public char[] ClientInfo; //1024

        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(Header);
            writer.Write(ClientInfo);
            return stream.ToArray();
        }

        public string GetName()
        {
            return "CONNECT REQUEST";
        }

        public string GetSequenceNumber()
        {
            return new string(ClientInfo);
        }

        public string GetRequestType()
        {
            return GetName();
        }

        public void SetRequestType(string requestType)
        {
            
        }

        public string GetMethodInvocationType()
        {
            return "None"; //TODO REMOVE HARDCODED
        }

        public void SetMethodInvocationType(string methodInvocationType)
        {

        }
    }
}