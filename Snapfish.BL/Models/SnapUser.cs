using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.BL.Models
{
    public class SnapUser
    {
        public long ID { get; set; }

        //public long AppUserID { get; set; }

        public string Email { get; set; }
        public string Name { get; set; }
    }
}
