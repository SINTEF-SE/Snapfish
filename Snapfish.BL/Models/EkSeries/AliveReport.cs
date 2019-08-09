using System.IO;

namespace Snapfish.BL.Models.EkSeries
{
    public struct AliveReport : ISendableStruct, IConvertable<AliveReport>
    {
        public char[] Header; // ALI\0
        public char[] Info; //sizeof (char * 1024 )

        public static AliveReport GenerateAliveReport(string clientId, int sequenceNumber)
        {
            return new AliveReport
            {
                Header = "ALI\0".ToCharArray(),
                Info = ("ClientID:" + clientId + ",SeqNo:" +
                        sequenceNumber + "\0").ToCharArray()
            };
        }

        public byte[] ToArray()
        {
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            writer.Write(Header);
            writer.Write(Info);
            return stream.ToArray();
        }
        
        public AliveReport FromArray(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            var s = default(AliveReport);
            s.Header = reader.ReadChars(4);
            s.Info = reader.ReadChars(1024);
            return s;
        }

        public string GetName()
        {
            return "ALIVE";
        }

        public string GetSequenceNumber()
        {
            return new string(Info);
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
            return "None"; // TODO: ENUM REMOVE HARDCODED
        }

        public void SetMethodInvocationType(string methodInvocationType)
        {
        }
    }
}