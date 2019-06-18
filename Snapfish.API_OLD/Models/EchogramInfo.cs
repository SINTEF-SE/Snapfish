using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.API.Models
{
    public class EchogramInfo
    {
        public long ID { get; set; }
        public long OwnerID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Biomass { get; set; }
        public string Source { get; set; }

        public string EchogramUrl { get; set; }

        public SnapUser Owner { get; set; }


    }
}
