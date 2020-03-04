using System.ComponentModel.DataAnnotations;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class BottomDetectionParameters
    {
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int UpperDetectorLimit = 0; // meter
        
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int LowerDetectorLimit = 1000; // meter
        
        [Range(-100, 100, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int BottomBackstep = -50; // dB
    }
}