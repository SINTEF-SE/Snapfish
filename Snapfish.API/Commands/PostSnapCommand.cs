using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.Controllers;
using Snapfish.API.Database;
using Snapfish.BL.Models;

namespace Snapfish.API.Commands
{
    public interface IPostSnapCommand : IAsyncCommand<SnapPacket>
    {
    }

    public class PostSnapCommand : IPostSnapCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public PostSnapCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }


        public async Task<IActionResult> ExecuteAsync(SnapPacket snapPacket, CancellationToken cancellationToken = default)
        {
            var snap = new Snap
            {
                SnapPacketJson = JsonConvert.SerializeObject(snapPacket)
            };

            _snapContext.Snaps.Add(snap);

            var metadata = new SnapMetadata
            {
                SnapId = snap.Id,
                OwnerId = snapPacket.OwnerId,
                Timestamp = snapPacket.Timestamp,
                Latitude = snapPacket.Latitude,
                Longitude = snapPacket.Longitude
            };

            await _snapContext.SnapMetadatas.AddAsync(metadata);
            await _snapContext.SaveChangesAsync();

            return new CreatedAtRouteResult(nameof(SnapController.GetSnap), new {id = snap.Id}, snapPacket);
        }
    }
}