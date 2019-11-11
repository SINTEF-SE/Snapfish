using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snapfish.BL.Models;

namespace Snapfish.BL.Models
{
    public class SnapMessage
    {
        public long Id { get; set; }
        public long OwnerId { get; set; }
        public long SenderId { get; set; }
        public string ReceiverEmails { get; set; }
        public string Message { get; set; }
        public DateTime SentTimestamp { get; set; }
        public long SnapMetadataId { get; set; }
        public bool Seen { get; set; }

        public SnapMetadata SnapMetadata { get; set; }
        public SnapUser Sender { get; set; }
    }
}
