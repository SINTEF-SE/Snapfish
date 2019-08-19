using System;

namespace Snapfish.BL.Models
{
    public class SnapMetadata
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Biomass { get; set; }
        public string Source { get; set; }
    }
}
