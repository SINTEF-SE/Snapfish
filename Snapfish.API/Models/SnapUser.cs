using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.API.Models
{
    public class SnapUser
    {
        public long ID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
