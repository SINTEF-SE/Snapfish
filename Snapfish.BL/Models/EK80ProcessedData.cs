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
            byte[] data = reader.ReadBytes(s.NoOfBytes);
            s.Data = new ushort[(int) Math.Ceiling((double) (data.Length / 2))];
            Buffer.BlockCopy(data, 0, s.Data, 0, data.Length);
            return s;
        }
    }
}