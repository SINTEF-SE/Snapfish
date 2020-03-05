using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public enum State
    {
        Start,
        Stop
    }

    public class NoiseSpectrum : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.NoiseSpectrum;
        
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meters

        public State State = State.Start;

        public NoiseSpectrum(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "Integration,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "Range=" + Range + ","
                      + "Rangestart=" + RangeStart + ","
                      + "State=" + State;
            return retval;
        }

        public EkSeriesDataSubscriptionType GetSubscriptionDataType()
        {
            return _ekSeriesDataSubscriptionType;
        }
    }
}