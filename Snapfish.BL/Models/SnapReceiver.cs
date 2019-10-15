using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.ComponentModel.DataAnnotations.Schema;

namespace Snapfish.API.Models
{
    public class SnapReceiver
    {
        public long SnapMessageID { get; set; }
        public long SnapUserID { get; set; }

        // TODO: Add status: unread, read, deleted

        // [NotMapped]
        public string ReceiverEmail { get; set; }
    }
}
