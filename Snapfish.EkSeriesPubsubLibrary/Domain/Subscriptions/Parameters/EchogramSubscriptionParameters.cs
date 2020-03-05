using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class EchogramSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.Echogram;
        [Range(0, 10000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int PixelCount = 500;

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meters

        public TVGType TVGType = TVGType.Sv;
        public EchogramType EchogramType = EchogramType.Surface;
        public CompressionType CompressionType = CompressionType.Mean;
        public ExpansionType ExpansionType = ExpansionType.Interpolation;

        public EchogramSubscriptionParameters(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "Echogram,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "PixelCount=" + PixelCount + ","
                      + "Range=" + Range + ","
                      + "RangeStart=" + RangeStart + ","
                      + "TVGType=" + TVGType + ","
                      + "EchogramType=" + EchogramType + ","
                      + "CompressionType="+ CompressionType + ","
                      + "ExpansionType=" +ExpansionType;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }
    }
}