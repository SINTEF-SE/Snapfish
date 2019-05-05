using System;
using System.IO;

namespace Snapfish.BL.Models
{
    public struct Ek80ProcessedData : IConvertable<Ek80ProcessedData>
    {
        public char[] Header; //PRD\0
        public long SeqNo;
        public long SubscriptionID;
        public ushort CurrentMsg;
        public ushort TotalMsg;
        public ushort NoOfBytes;
        public ushort[] Data;
        // THIS IS NOT IN THE MANUAL BUT FOR FURTHER TRANSMISSION WE ACTUALLY NEED AN INTERMEDIATE FORMAT SO THIS 'ADDED' DATA is needed and thus not redundant
        public byte[] DataAsBytes;

        public Ek80ProcessedData FromArray(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            var s = default(Ek80ProcessedData);
            s.Header = reader.ReadChars(4);
            s.SeqNo = reader.ReadInt32();
            s.SubscriptionID = reader.ReadInt32();
            s.CurrentMsg = reader.ReadUInt16();
            s.TotalMsg = reader.ReadUInt16();
            s.NoOfBytes = reader.ReadUInt16();
            s.DataAsBytes = reader.ReadBytes(s.NoOfBytes);
            s.Data = new ushort[(int) Math.Ceiling((double) (s.DataAsBytes.Length / 2))];
            Buffer.BlockCopy(s.DataAsBytes, 0, s.Data, 0, s.DataAsBytes.Length);
            return s;
        }
    }
}