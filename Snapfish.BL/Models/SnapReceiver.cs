using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using System.ComponentModel.DataAnnotations.Schema;

namespace Snapfish.BL.Models
{
    public class SnapReceiver
    {
        public long SnapMessageID { get; set; }
        public long SnapUserID { get; set; }

        //public bool Seen { get; set; }

        //public bool Deleted { get; set; }

        // [NotMapped]
        public string ReceiverEmail { get; set; }
    }
}
