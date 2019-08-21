using System.ComponentModel.DataAnnotations;

namespace Snapfish.BL.Models
{
    public class Snap
    {
        [Key]
        public long Id { get; set; }
        public string SnapPacketJson { get; set; }
    }
}