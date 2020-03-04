using System.ComponentModel.DataAnnotations;
using Snapfish.BL.Models.EkSeries.Parameters;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public class EchogramParameters
    {
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
    }
}