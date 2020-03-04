using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class TargetsIntegrationParameters
    {
        public LayerType LayerType = LayerType.Surface;
        public IntegrationState IntegrationState = IntegrationState.Start;
        public Update Update = Update.UpdatePing;

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 10; // meters

        [Range(0, 200, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Margin = 1; // meters

        [Range(-200, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int SvThreshold = -100; // dB

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
    }
}