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
                new EchogramInfo{UserID=1, Latitude="632448", Longitude="102433", Source="EK80", EchogramUrl="https://www.sintef.no", Timestamp=DateTime.Parse("2019-05-12 08:01+0100"), Biomass=""},
                new EchogramInfo{UserID=1, Latitude="632451", Longitude="102114", Source="EK80", EchogramUrl="https://www.nrk.no", Timestamp=DateTime.Parse("2019-05-12 09:21+0100"), Biomass=""},
                new EchogramInfo{UserID=1, Latitude="632451", Longitude="102114", Source="EK80", EchogramUrl="https://www.dagbladet.no", Timestamp=DateTime.Parse("2019-05-12 09:26+0100"), Biomass=""}
            };
            foreach (EchogramInfo e in echograms)
            {
                context.EchogramInfos.Add(e);
            }
            context.SaveChanges();


            var snaps = new SnapMessage[]
            {
                new SnapMessage{EchogramInfoID = echograms[0].ID, Title="Mye fisk her", Comment="Vi har ikke plass til mer", Sender="Per", Receivers="Ola", SendTimestamp=DateTime.Parse("2019-05-12 08:04+0100"), SharePublicly=false},
                new SnapMessage{EchogramInfoID = echograms[1].ID, Title="Torsk", Comment="Her blir det fangst!", Sender="Jens", Receivers="Ola", SendTimestamp=DateTime.Parse("2019-05-12 09:32+0100"), SharePublicly=true}
            };
            foreach (SnapMessage s in snaps)
            {
                context.SnapMessages.Add(s);
            }
            context.SaveChanges();

        }
    }
}
