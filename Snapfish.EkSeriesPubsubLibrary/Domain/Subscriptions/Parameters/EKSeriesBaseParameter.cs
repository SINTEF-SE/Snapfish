namespace Snapfish.EkSeriesPubsubLibrary.Domain.Subscriptions.Parameters
{
    public abstract class EKSeriesBaseParameter
    {
        private string _channelId;

        public EKSeriesBaseParameter(string channelId)
        {
            _channelId = channelId;
        }

        public string GetChannelId()
        {
            return _channelId ?? "";
        }
    }
}