using Snapfish.BL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snapfish.API.Database
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


            var metadata = new SnapMetadata[]
            {
                new SnapMetadata{OwnerId=users[0].ID, Latitude="632448", Longitude="102433", Source="EK80", Timestamp=DateTime.Parse("2019-05-12 08:01+0100"), Biomass="", SharePublic = false},
                new SnapMetadata{OwnerId=users[0].ID, Latitude="632451", Longitude="102114", Source="EK80", Timestamp=DateTime.Parse("2019-05-12 09:21+0100"), Biomass="", SharePublic = true, SharePublicFrom=DateTime.Parse("2019-05-12 11:21+0100")}
            };
            foreach (SnapMetadata e in metadata)
            {
                context.SnapMetadatas.Add(e);
            }
            context.SaveChanges();


            var snaps = new SnapMessage[]
            {
                new SnapMessage{SnapMetadataId = metadata[0].Id, Message="Mye fisk her! Vi har ikke plass til mer", OwnerId=users[1].ID, SenderId=users[0].ID,ReceiverEmails="ola@fiskinfo.no", SentTimestamp=DateTime.Parse("2019-05-12 08:04+0100")},
                new SnapMessage{SnapMetadataId = metadata[0].Id, Message="Mye fisk her! Vi har ikke plass til mer", OwnerId=users[0].ID, SenderId=users[0].ID,ReceiverEmails="ola@fiskinfo.no", SentTimestamp=DateTime.Parse("2019-05-12 08:04+0100")},
                new SnapMessage{SnapMetadataId = metadata[1].Id, Message="Her blir det torsk til middag!",  OwnerId=users[0].ID, SenderId=users[2].ID, ReceiverEmails="per@fiskinfo.no, ola@fiskinfo.no", SentTimestamp=DateTime.Parse("2019-05-12 09:32+0100")},
                new SnapMessage{SnapMetadataId = metadata[1].Id, Message="Her blir det torsk til middag!",  OwnerId=users[1].ID, SenderId=users[2].ID, ReceiverEmails="per@fiskinfo.no, ola@fiskinfo.no", SentTimestamp=DateTime.Parse("2019-05-12 09:32+0100")},
                new SnapMessage{SnapMetadataId = metadata[1].Id, Message="Her blir det torsk til middag!",  OwnerId=users[2].ID, SenderId=users[2].ID, ReceiverEmails="per@fiskinfo.no, ola@fiskinfo.no", SentTimestamp=DateTime.Parse("2019-05-12 09:32+0100")}
            };
            foreach (SnapMessage s in snaps)
            {
                context.SnapMessages.Add(s);
            }
            context.SaveChanges();

            /*
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
            */
        }
    }
}
