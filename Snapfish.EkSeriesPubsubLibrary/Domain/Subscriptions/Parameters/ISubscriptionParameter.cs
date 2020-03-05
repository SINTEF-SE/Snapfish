using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public interface ISubscriptionParameter
    {
        string CreateSubscribableMethodInvocationString();
        EkSeriesDataSubscriptionType GetSubscriptionDataType();
    }
}