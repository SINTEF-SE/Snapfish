using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class TargetsEchogramSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.TargetsEchogram;
        
        [Range(0, 10000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int PixelCount = 500;

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meters

        public TVGType TVGType = TVGType.Sv;
        public EchogramType EchogramType = EchogramType.Surface;

        [Range(-120, 50, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MinTSValue = -50.0; // decibel

        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MinEchoLength = 0.8;

        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MaxEchoLength = 1.8;

        [Range(0, 12, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MaxGainCompensation = 6.0;

        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MaxPhaseDeviation = 8.0;

        public TargetsEchogramSubscriptionParameters(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "TargetsEchogram,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "PixelCount=" + PixelCount + ","
                      + "Range=" + Range + ","
                      + "RangeStart=" + RangeStart + ","
                      + "TVGType=" + TVGType + ","
                      + "EchogramType=" + EchogramType + ","
                      + "MinTSValue=" + MinTSValue + ","
                      + "MinEcholength=" + MinEchoLength + ","
                      + "MaxEcholength=" + MaxEchoLength + ","
                      + "MaxGainCompensation=" + MaxGainCompensation + ","
                      + "MaxPhasedeviation=" + MaxPhaseDeviation;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }
    }
}