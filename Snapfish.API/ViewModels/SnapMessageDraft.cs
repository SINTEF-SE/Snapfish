using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snapfish.BL.Models;

namespace Snapfish.API.ViewModels
{
    public class SnapMessageDraft
    {
        public string SenderEmail { get; set; }
        public string ReceiverEmails { get; set; }
        public string Message { get; set; }
        public long SnapMetadataId { get; set; }
    }
}
