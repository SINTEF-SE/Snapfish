using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class BottomDetectionSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.BottomDetection;
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int UpperDetectorLimit = 0; // meter

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int LowerDetectorLimit = 1000; // meter

        [Range(-100, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int BottomBackstep = -50; // dB

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";

            retval += "BottomDetection,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "UpperDetectorLimit=" + UpperDetectorLimit + ","
                      + "LowerDetectorLimit=" + LowerDetectorLimit + ","
                      + "BottomBackstep=" + BottomBackstep;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }

        public BottomDetectionSubscriptionParameters(string channelId) : base(channelId)
        {
        }
    }
}