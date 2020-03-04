using System.ComponentModel.DataAnnotations;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public enum State
    {
        Start, Stop
    }
    
    public class NoiseSpectrum
    {
        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int Range = 100; // meters

        [Range(0, 20000, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int RangeStart = 0; // meters

        public State State = State.Start;
    }
}