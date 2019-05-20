namespace Snapfish.BL.Models
{
    struct StructSampleDataHeader
    {
        public ulong dlTime; //DWORDLONG
    }

    
    // Sample data for Power, Angle, Sv, Sp, Ss, TVG20 and TVG40
    public struct SampleDataContainerClass
    {
        StructSampleDataHeader SampleDataHeader;
        StructSampleDataArray SampleDataArray;
    }
    

    struct StructSampleDataArray
    {
        short[] nSampleDataElement; // [30000]  16-bits sample in logarithmic format
    }
}