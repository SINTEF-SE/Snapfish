using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public enum SampleDataType
    {
        Power,
        Angle,
        Sv,
        Sp,
        Ss,
        TVG20,
        TBG40,
        PowerAngle
    }

    public class SampleDataSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.SampleData;
        public SampleDataType SampleDataType = SampleDataType.Power;

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 10000; // meter

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meter

        public SampleDataSubscriptionParameters(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "SampleData,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "SampleDataType="+ SampleDataType + ","
                      + "Range=" + Range + ","
                      + "RangeStart=" + RangeStart;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }
    }
}