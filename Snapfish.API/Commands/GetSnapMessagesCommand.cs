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

        public async Task<IActionResult> ExecuteAsync(int userID, bool inbox,  bool snapmetadata, CancellationToken cancellationToken = default)
        {
            List<SnapMessage> messages = null;


            if (inbox)
            {
                if (snapmetadata)
                {
                    messages = await _snapContext.SnapMessages
                        .Where(msg => (msg.OwnerID == userID) && (msg.SenderID != userID))
                                .Include(s => s.SnapMetadata)
                                .Include(s => s.Sender)
                                .ToListAsync();
                } else
                {
                    messages = await _snapContext.SnapMessages
                        .Where(msg => (msg.OwnerID == userID) && (msg.SenderID != userID))
                        .Include(s => s.Sender)
                        .ToListAsync();

                }
            } else
            {   // outbox
                if (snapmetadata)
                {
                    messages = await _snapContext.SnapMessages
                        .Where(msg => (msg.OwnerID == userID) && (msg.SenderID == userID))
                                .Include(s => s.SnapMetadata)
                                .Include(s => s.Sender)
                                .ToListAsync();
                }
                else
                {
                    messages = await _snapContext.SnapMessages
                        .Where(msg => (msg.OwnerID == userID) && (msg.SenderID == userID))
                        .Include(s => s.Sender)
                        .ToListAsync();

                }
            }

            if (messages == null)
                return new NotFoundResult();
            return new OkObjectResult(messages);
        }
    }
}
