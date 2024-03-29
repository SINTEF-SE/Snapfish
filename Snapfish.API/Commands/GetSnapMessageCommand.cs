﻿using System;
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

    public interface IGetSnapMessageCommand : IAsyncCommand<long>
    {
    }

    public class GetSnapMessageCommand : IGetSnapMessageCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public GetSnapMessageCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

        public async Task<IActionResult> ExecuteAsync(long id, CancellationToken cancellationToken = default)
        {
            var message = await _snapContext.SnapMessages
                    .FindAsync(id);

            //TODO: Add check for access right (either as owner, snap receier, or public)
            if (message == null)
                return new NotFoundResult();
            return new OkObjectResult(message);
        }
    }
}
