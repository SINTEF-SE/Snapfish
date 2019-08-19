using System;

namespace Snapfish.BL.Models
{
    
    public struct Slice
    {
        public ulong Timestamp { get; set; }
        public double Range { get; set; }
        public double RangeStart { get; set; }
        public int DataLength { get; set; }
        public short[] Data { get; set; }
    }
    
    public class SnapPacket
    {   
        public long OwnerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Slice[] Slices { get; set; }
        public int NumberOfSlices { get; set; }
        public int SliceHeight { get; set; }
        public string Biomass { get; set; }
    }
    
}