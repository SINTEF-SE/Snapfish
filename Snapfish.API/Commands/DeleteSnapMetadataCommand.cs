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

    public interface IDeleteSnapMetadataCommand : IAsyncCommand<long>
    {
    }

    public class DeleteSnapMetadataCommand : IDeleteSnapMetadataCommand
    {
        private readonly SnapContext _snapContext;

        public DeleteSnapMetadataCommand(SnapContext snapContext) => _snapContext = snapContext;
        
  
        public async Task<IActionResult> ExecuteAsync(long id, CancellationToken cancellationToken = default)
        {
            var echogramInfo = await _snapContext.SnapMetadatas.FindAsync(id);

            if (echogramInfo == null)
            {
                return new NotFoundResult(); // NotFound vs. NotFoundResult ?
            }

            _snapContext.SnapMetadatas.Remove(echogramInfo);
            await _snapContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}
