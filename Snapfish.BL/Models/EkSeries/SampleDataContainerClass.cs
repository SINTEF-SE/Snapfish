using System;
using System.IO;
using Snapfish.BL.Extensions;

namespace Snapfish.BL.Models.EkSeries
{
    struct StructSampleDataHeader
    {
        public ulong dlTime; //DWORDLONG
    }
    
    struct StructSampleDataArray
    {
        public short[] nSampleDataElement; // [30000]  16-bits sample in logarithmic format
    }


    // Sample data for Power, Angle, Sv, Sp, Ss, TVG20 and TVG40
    public struct SampleDataContainerClass
    {
        StructSampleDataHeader SampleDataHeader;
        StructSampleDataArray SampleDataArray;
        
        public static SampleDataContainerClass FromArray(byte[] bytes)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            SampleDataContainerClass s = default(SampleDataContainerClass);
            StructSampleDataHeader header = new StructSampleDataHeader {dlTime = reader.ReadUInt64()};
            s.SampleDataHeader = header;
            StructSampleDataArray array = new StructSampleDataArray();
            byte[] remainingDataFromReader = reader.ReadAllBytes();
            array.nSampleDataElement = new short[(int) Math.Ceiling((double) (remainingDataFromReader.Length / 2))];
            Buffer.BlockCopy(remainingDataFromReader, 0, array.nSampleDataElement, 0, remainingDataFromReader.Length);
            s.SampleDataArray = array;
            return s;
        }
    }
}