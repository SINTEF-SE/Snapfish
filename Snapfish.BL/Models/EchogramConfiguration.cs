namespace Snapfish.BL.Models
{
    public class EchogramConfiguration
    {
        public string PixelCount { get; set; }
        public string Range { get; set; }
        public string RangeStart { get; set; }

        public EchogramConfiguration(string pixelCount = "500", string range = "100", string rangeStart = "0")
        {
            PixelCount = pixelCount;
            Range = range;
            RangeStart = rangeStart;
        }
    }
}