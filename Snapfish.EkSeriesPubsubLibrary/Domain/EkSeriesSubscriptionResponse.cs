using Snapfish.BL.Models;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class EkSeriesSubscriptionResponse
    {
        public int RequestId { get; set; }
        public int SubscriptionId { get; set; }
        public EkSeriesDataSubscriptionType SubscriptionType { get; set; }
    }
}