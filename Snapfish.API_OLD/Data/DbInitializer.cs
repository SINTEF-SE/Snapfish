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

            var users = new SnapUser[]
            {
                new SnapUser{Name="Per", Email="per@fiskinfo.no"},
                new SnapUser{Name="Ola", Email="ola@fiskinfo.no"},
                new SnapUser{Name="Jens", Email="jens@fiskinfo.no"}
            };
            foreach (SnapUser e in users)
            {
                context.SnapUsers.Add(e);
            }
            context.SaveChanges();


            var echograms = new EchogramInfo[]
            {
                new EchogramInfo{OwnerID=users[0].ID, Latitude="632448", Longitude="102433", Source="EK80", EchogramUrl="https://www.sintef.no", Timestamp=DateTime.Parse("2019-05-12 08:01+0100"), Biomass=""},
                new EchogramInfo{OwnerID=users[0].ID, Latitude="632451", Longitude="102114", Source="EK80", EchogramUrl="https://10.218.87.81:5006/test/index.html", Timestamp=DateTime.Parse("2019-05-12 09:21+0100"), Biomass=""}
//                new EchogramInfo{OwnerID=users[0].ID, Latitude="632451", Longitude="102114", Source="EK80", EchogramUrl="https://www.dagbladet.no", Timestamp=DateTime.Parse("2019-05-12 09:26+0100"), Biomass=""},
//                new EchogramInfo{OwnerID=users[1].ID, Latitude="642451", Longitude="102114", Source="EK80", EchogramUrl="https://www.adressa.no", Timestamp=DateTime.Parse("2019-05-12 09:21+0100"), Biomass=""},
//                new EchogramInfo{OwnerID=users[1].ID, Latitude="642451", Longitude="102114", Source="EK80", EchogramUrl="https://www.aftenposten.no", Timestamp=DateTime.Parse("2019-05-13 09:26+0100"), Biomass=""}
            };
            foreach (EchogramInfo e in echograms)
            {
                context.EchogramInfos.Add(e);
            }
            context.SaveChanges();


            var snaps = new SnapMessage[]
            {
                new SnapMessage{EchogramInfoID = echograms[0].ID, Title="Mye fisk her", Comment="Vi har ikke plass til mer", SenderID=users[0].ID, SenderEmail=users[0].Email, SendTimestamp=DateTime.Parse("2019-05-12 08:04+0100"), SharePublicly=false},
                new SnapMessage{EchogramInfoID = echograms[1].ID, Title="Torsk", Comment="Her blir det fangst!", SenderID=users[2].ID, SenderEmail=users[2].Email, SendTimestamp=DateTime.Parse("2019-05-12 09:32+0100"), SharePublicly=true}
            };
            foreach (SnapMessage s in snaps)
            {
                context.SnapMessages.Add(s);
            }
            context.SaveChanges();

            var snapReceivers = new SnapReceiver[]
            {
                new SnapReceiver{SnapMessageID=snaps[0].ID, SnapUserID=users[1].ID, ReceiverEmail=users[1].Email},
                new SnapReceiver{SnapMessageID=snaps[1].ID, SnapUserID=users[0].ID, ReceiverEmail=users[0].Email},
                new SnapReceiver{SnapMessageID=snaps[1].ID, SnapUserID=users[1].ID, ReceiverEmail=users[1].Email},
            };
            foreach (SnapReceiver sr in snapReceivers)
            {
                context.SnapReceivers.Add(sr);
            }
            context.SaveChanges();

        }
    }
}
