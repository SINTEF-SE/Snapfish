using System.ComponentModel.DataAnnotations;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{

    public enum SampleDataType
    {
        Power, Angle, Sv, Sp, Ss, TVG20, TBG40, PowerAngle
    }
    public class SampleDataParameters
    {
        public SampleDataType SampleDataType = SampleDataType.Power;
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 10000; // meter
        
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meter
    }
}