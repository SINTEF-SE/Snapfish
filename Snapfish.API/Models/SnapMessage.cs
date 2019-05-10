using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.API.Models
{
    public class SnapMessage
    {
        public long Id { get; set; }
        public string Sender { get; set; }
        public string Receivers { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime SendTimestamp { get; set; }
        public bool SharePublicly { get; set; }
        public long EchogramInfoId { get; set; }
    }
}
