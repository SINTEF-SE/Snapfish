using Snapfish.BL.Models;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class EkSeriesSubscriptionResponse
    {
        public int RequestId { get; set; }
        public int SubscriptionId { get; set; }
        public EkSeriesDataSubscriptionType SubscriptionType { get; set; }
    }
}