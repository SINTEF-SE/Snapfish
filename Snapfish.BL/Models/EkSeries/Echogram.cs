using System;
using System.IO;
using Snapfish.BL.Extensions;

namespace Snapfish.BL.Models.EkSeries
{
    public struct EchogramHeader
    {
        public ulong dlTime; //DWORDLONG
        public double range;
        public double rangeStart;
    }

    public struct EchogramArray
    {
        public short[] nEchogramElement; //[30000]        
    }

    public struct Echogram
    {
        public EchogramHeader EchogramHeader;
        public EchogramArray EchogramArray;

        public static Echogram FromArray(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            Echogram s = default(Echogram);
            EchogramHeader header = new EchogramHeader {dlTime = reader.ReadUInt64(), range = reader.ReadDouble(), rangeStart = reader.ReadDouble()};

            s.EchogramHeader = header;
            
            EchogramArray array = new EchogramArray();
            byte[] remainingDataFromReader = reader.ReadAllBytes();
            array.nEchogramElement = new short[(int) Math.Ceiling((double) (remainingDataFromReader.Length / 2))]; //I know the documentation says new short[30000], but the ram!
            Buffer.BlockCopy(remainingDataFromReader, 0, array.nEchogramElement, 0, remainingDataFromReader.Length);
            s.EchogramArray = array;
            return s;
        }
    }
}