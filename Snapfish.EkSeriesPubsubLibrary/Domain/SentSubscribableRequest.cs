using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Snapfish.BL.Models.EkSeries;

namespace Snapfish.EkSeriesPubsubLibrary.Domain
{
    public class SentSubscribableRequest
    {
        public EkSeriesDataSubscriptionType SubscriptionType { get; set; }
        public CommandRequest SubscriptionRequest { get; set; } 
        public long Id { get; set; }
        private Stopwatch _timer;

        public SentSubscribableRequest (EkSeriesDataSubscriptionType subscriptionType, long id, CommandRequest subscriptionRequest)
        {
            SubscriptionType = subscriptionType;
            SubscriptionRequest = subscriptionRequest;
            Id = id;
            _timer = new Stopwatch();
            _timer.Start();
        }

        public void ResetStopWatch()
        {
            _timer.Restart();
        }

        public long GetElapsedTimeInMilliseconds()
        {
            return _timer.ElapsedMilliseconds;
        }
    }
}