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

    public interface IPostSnapMessageCommand : IAsyncCommand<SnapMessage>
    {
    }

    public class PostSnapMessageCommand : IPostSnapMessageCommand
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly SnapContext _snapContext;

        public PostSnapMessageCommand(IActionContextAccessor actionContextAccessor, SnapContext snapContext)
        {
            _actionContextAccessor = actionContextAccessor;
            _snapContext = snapContext;
        }

  
        public async Task<IActionResult> ExecuteAsync(SnapMessage message, CancellationToken cancellationToken = default)
        {
            message.ID = 0;
            message.EchogramInfo = null;
            message.SendTimestamp = DateTime.Now;

            // Look up sender
            SnapUser sendingUser = _snapContext.SnapUsers.Where(s => s.Email == message.SenderEmail).First();
            if (sendingUser != null)
            {
                message.SenderID = sendingUser.ID;
            } else
            {
                return new BadRequestResult();
            }

            // Look up receivers
            foreach (SnapReceiver receiver in message.Receivers)
            {
                SnapUser user = _snapContext.SnapUsers.Where(s => s.Email == receiver.ReceiverEmail).First();
                if (user != null)
                {
                    receiver.SnapUserID = user.ID;
                }
                // TODO: consider what to do if one of the receivers were not found
            }

            await _snapContext.SnapMessages.AddAsync(message);
            await _snapContext.SaveChangesAsync();

            return new CreatedAtRouteResult(nameof(SnapMessagesController.GetSnapMessage), new { id = message.ID }, message); 
        }
    }
}
