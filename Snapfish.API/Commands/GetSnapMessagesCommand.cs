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
using Snapfish.BL.Models;

namespace Snapfish.API.Commands
{

    public interface IGetSnapMessagesCommand : IAsyncCommand<int, bool, bool>
    {
    }

    public class GetSnapMessagesCommand : IGetSnapMessagesCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public GetSnapMessagesCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

        public async Task<IActionResult> ExecuteAsync(int userID, bool withEchogram, bool withReceivers, CancellationToken cancellationToken = default)
        {

            List<SnapMessage> messages = null;

            if (withReceivers && withEchogram)
            {
                messages = await _snapContext.SnapMessages
                                .Include(s => s.EchogramInfo)
                                .Include(s => s.Sender)
                                .Include(s => s.Receivers)
                                .ToListAsync();
            } else if (withEchogram)
            {
                messages = await _snapContext.SnapMessages
                                .Include(s => s.EchogramInfo)
                                .Include(s => s.Sender)
                                .ToListAsync();
            } else if (withReceivers) 
            {
                messages = await _snapContext.SnapMessages
                                .Include(s => s.Sender)
                                .Include(s => s.Receivers)
                                .ToListAsync();
            } else
            {
                messages = await _snapContext.SnapMessages
                                .Include(s => s.Sender)
                                .ToListAsync();
            }

            // TODO: add filter on user?                            .Where(message => message.OwnerId == userID)

            if (messages == null)
                return new NotFoundResult();
            return new OkObjectResult(messages);
        }
    }
}
