using Snapfish.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.API.Data
{
    public static class DbInitializer
    {
        public static void Initialize(SnapContext context)
        {
            context.Database.EnsureCreated();

            if (context.SnapMessages.Any())
            {
                return;
            }

            var echograms = new EchogramInfo[]
            {
                new EchogramInfo{ID=1, UserID=1, Latitude="632448", Longitude="102433", Source="EK80", EchogramUrl="https://www.sintef.no", Timestamp=DateTime.Parse("2019-05-12 08:01"), Biomass=""},
                new EchogramInfo{ID=2, UserID=1, Latitude="632451", Longitude="102114", Source="EK80", EchogramUrl="https://www.nrk.no", Timestamp=DateTime.Parse("2019-05-12 09:21"), Biomass=""}
            };
            foreach (EchogramInfo e in echograms)
            {
                context.EchogramInfos.Add(e);
            }
            context.SaveChanges();


            var snaps = new SnapMessage[]
            {
                new SnapMessage{ID=1, EchogramInfoID = 1, Title="Mye fisk her", Comment="Vi har ikke plass til mer", Sender="Per", Receivers="Ola", SendTimestamp=DateTime.Parse("2019-05-12 08:04"), SharePublicly=false},
                new SnapMessage{ID=2, EchogramInfoID = 2, Title="Torsk", Comment="Her blir det fangst!", Sender="Jens", Receivers="Ola", SendTimestamp=DateTime.Parse("2019-05-12 09:32"), SharePublicly=true}
            };
            foreach (SnapMessage s in snaps)
            {
                context.SnapMessages.Add(s);
            }
            context.SaveChanges();

        }
    }
}
