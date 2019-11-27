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

    public interface IDeleteSnapMessageCommand : IAsyncCommand<long>
    {
    }

    public class DeleteSnapMessageCommand : IDeleteSnapMessageCommand
    {
        private readonly SnapContext _snapContext;

        public DeleteSnapMessageCommand(SnapContext snapContext) => _snapContext = snapContext;
        
  
        public async Task<IActionResult> ExecuteAsync(long id, CancellationToken cancellationToken = default)
        {
            var message = await _snapContext.SnapMessages.FindAsync(id);

            if (message == null)
            {
                return new NotFoundResult(); // NotFound vs. NotFoundResult ?
            }

            _snapContext.SnapMessages.Remove(message);
            await _snapContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}
