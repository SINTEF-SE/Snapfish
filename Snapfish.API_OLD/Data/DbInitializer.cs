using System;
using System.Linq;
using Snapfish.API.Models;
using Snapfish.BL.Models;
using SnapMessage = Snapfish.API.Models.SnapMessage;
using SnapUser = Snapfish.API.Models.SnapUser;

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
                new SnapUser {Name = "Per", Email = "per@fiskinfo.no"},
                new SnapUser {Name = "Ola", Email = "ola@fiskinfo.no"},
                new SnapUser {Name = "Jens", Email = "jens@fiskinfo.no"}
            };
            foreach (SnapUser e in users)
            {
                context.SnapUsers.Add(e);
            }

            context.SaveChanges();


            var echograms = new SnapMetadata[]
            {
                new SnapMetadata
                {
                    OwnerId = users[0].ID, Latitude = "632448", Longitude = "102433", Source = "EK80", Timestamp = DateTime.Parse("2019-05-12 08:01+0100"), Biomass = ""
                },
                new SnapMetadata
                {
                    OwnerId = users[0].ID, Latitude = "632451", Longitude = "102114", Source = "EK80", Timestamp = DateTime.Parse("2019-05-12 09:21+0100"), Biomass = ""
                }
            };
            foreach (SnapMetadata e in echograms)
            {
                context.SnapMetadatas.Add(e);
            }

            context.SaveChanges();


            var snaps = new SnapMessage[]
            {
                new SnapMessage
                {
                    EchogramInfoID = echograms[0].Id, Title = "Mye fisk her", Comment = "Vi har ikke plass til mer", SenderID = users[0].ID, SenderEmail = users[0].Email,
                    SendTimestamp = DateTime.Parse("2019-05-12 08:04+0100"), SharePublicly = false
                },
                new SnapMessage
                {
                    EchogramInfoID = echograms[1].Id, Title = "Torsk", Comment = "Her blir det fangst!", SenderID = users[2].ID, SenderEmail = users[2].Email,
                    SendTimestamp = DateTime.Parse("2019-05-12 09:32+0100"), SharePublicly = true
                }
            };
            foreach (SnapMessage s in snaps)
            {
                context.SnapMessages.Add(s);
            }

            context.SaveChanges();

            var snapReceivers = new SnapReceiver[]
            {
                new SnapReceiver {SnapMessageID = snaps[0].ID, SnapUserID = users[1].ID, ReceiverEmail = users[1].Email},
                new SnapReceiver {SnapMessageID = snaps[1].ID, SnapUserID = users[0].ID, ReceiverEmail = users[0].Email},
                new SnapReceiver {SnapMessageID = snaps[1].ID, SnapUserID = users[1].ID, ReceiverEmail = users[1].Email},
            };
            foreach (SnapReceiver sr in snapReceivers)
            {
                context.SnapReceivers.Add(sr);
            }

            context.SaveChanges();
        }
    }
}