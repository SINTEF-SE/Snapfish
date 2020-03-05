using System;
using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class TargetStrengthChirpSubscriptionParameters : EKSeriesBaseParameter, ISubscriptionParameter
    {
        private EkSeriesDataSubscriptionType _ekSeriesDataSubscriptionType = EkSeriesDataSubscriptionType.TargetStrengthTsDetectionChirp;
        public LayerType sLayerType = LayerType.Surface;

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 10000; // meter

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meter

        [Range(-120, 50, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MinTSValue = -50.0; // decibel

        [Range(0, Double.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double RangeBeforeTarget = 0.15; // Meter

        [Range(0, 20, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double RangeAfterTarget = 0.15; // Meter

        [Range(0, 12, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public double MaxGainCompensation = 6.0;

        [Range(0, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int MaxPhaseDeviation = 25; //Degree

        public TargetStrengthChirpSubscriptionParameters(string channelId) : base(channelId)
        {
        }

        public string CreateSubscribableMethodInvocationString()
        {
            string retval = "";
            retval += "TSDetectionChirp,"
                      + "ChannelID=" + GetChannelId() + ","
                      + "LayerType=" + sLayerType + ","
                      + "Range=" + Range + ","
                      + "RangeStart=" + RangeStart + ","
                      + "MinTSValue=" + MinTSValue + ","
                      + "RangeBeforeTarget=" + RangeBeforeTarget + ","
                      + "RangeAfterTarget=" + RangeAfterTarget + ","
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