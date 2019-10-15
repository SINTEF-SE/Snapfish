using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snapfish.BL.Models;

namespace Snapfish.BL.Models
{
    public class SnapMessage
    {
        public long ID { get; set; }
        public long SenderID { get; set; }
        public string SenderEmail { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public DateTime SendTimestamp { get; set; }
        public bool SharePublicly { get; set; }
        public long EchogramInfoID { get; set; }

        public SnapMetadata EchogramInfo { get; set; }
        public SnapUser Sender { get; set; }
        public List<SnapReceiver> Receivers { get; set; }
    }
}
