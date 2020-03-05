using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class IntegrationSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.Integration;
        
        public LayerType LayerType = LayerType.Surface;
        public IntegrationState IntegrationState = IntegrationState.Start;
        public Update Update = BL.Models.EkSeries.Parameters.Update.UpdatePing;
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 10; // meters
        
        [Range(0, 200, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Margin = 1; // meters
        
        [Range(-200, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int SvThreshold = -100; // dB

        public IntegrationSubscriptionParameters(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "Integration,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "State=" + IntegrationState + ","
                      + "Update=" + Update + ","
                      + "Layertype=" + LayerType + ","
                      + "Range=" + Range + ","
                      + "Rangestart=" + RangeStart + ","
                      + "Margin=" + Margin + ","
                      + "SvThreshold=" + SvThreshold;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }
    }
}