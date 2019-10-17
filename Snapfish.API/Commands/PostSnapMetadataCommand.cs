using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using SintefSecure.Framework.SintefSecure.AspNetCore;
using Snapfish.API.Controllers;
using Snapfish.API.Database;
using Snapfish.BL.Models;

namespace Snapfish.API.Commands
{

    public interface IPostSnapMetadataCommand : IAsyncCommand<SnapMetadata>
    {
    }

    public class PostSnapMetadataCommand : IPostSnapMetadataCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public PostSnapMetadataCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

  
        public async Task<IActionResult> ExecuteAsync(SnapMetadata metadata, CancellationToken cancellationToken = default)
        {

            _snapContext.SnapMetadatas.Add(metadata);
            await _snapContext.SaveChangesAsync();

            return new CreatedAtRouteResult(nameof(SnapMetadataController.GetSnapMetadata), new { id = metadata.Id }, metadata); 
        }
    }
}
