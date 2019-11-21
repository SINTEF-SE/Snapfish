using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.Database;
using Snapfish.BL.Models;

namespace Snapfish.API.Commands
{

    public interface IGetSnapCommand : IAsyncCommand<long>
    {
    }

    public class GetSnapCommand : IGetSnapCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public GetSnapCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

        public async Task<IActionResult> ExecuteAsync(long id, CancellationToken cancellationToken = default)
        {
            var info = await _snapContext.Snaps
                    .FindAsync(id);

            //TODO: Add check for access right (either as owner, snap receier, or public)
            if (info == null)
                return new NotFoundResult();


            var packet = JsonConvert.DeserializeObject<SnapPacket>(info.SnapPacketJson);

            return new OkObjectResult(packet);
        }
    }
}
