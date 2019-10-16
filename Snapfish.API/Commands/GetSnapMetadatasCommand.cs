using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.Database;

namespace Snapfish.API.Commands
{

    public interface IGetSnapMetadatasCommand : IAsyncCommand<int>
    {
    }

    public class GetSnapMetadatasCommand : IGetSnapMetadatasCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public GetSnapMetadatasCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

        public async Task<IActionResult> ExecuteAsync(int userID, CancellationToken cancellationToken = default)
        {
            var infos = await _snapContext.SnapMetadatas
                            .Where(info => info.OwnerId == userID)
                            .ToListAsync();
            if (infos == null)
                return new NotFoundResult();
            return new OkObjectResult(infos);
        }
    }
}
