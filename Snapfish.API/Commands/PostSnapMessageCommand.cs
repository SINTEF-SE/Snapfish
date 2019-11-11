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
using Snapfish.API.ViewModels;
using Snapfish.BL.Models;

namespace Snapfish.API.Commands
{

    public interface IPostSnapMessageCommand : IAsyncCommand<SnapMessageDraft>
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


        public async Task<IActionResult> ExecuteAsync(SnapMessageDraft messageDraft, CancellationToken cancellationToken = default)
        {
            DateTime stamp = DateTime.Now;

            // Look up sender
            SnapUser sendingUser = _snapContext.SnapUsers.Where(s => s.Email == messageDraft.SenderEmail).First();
            SnapMessage sendersMessage;
            if (sendingUser != null)
            {
                var message = new SnapMessage
                {
                    OwnerId = sendingUser.ID,
                    SenderId = sendingUser.ID,
                    ReceiverEmails = messageDraft.ReceiverEmails,
                    Message = messageDraft.Message,
                    SentTimestamp = stamp,
                    Seen = false,
                    SnapMetadataId = messageDraft.SnapMetadataId
                };
                sendersMessage = message;
                await _snapContext.SnapMessages.AddAsync(message);

            } else
            {
                return new BadRequestResult();
            }

            char[] separators = new char[] { ',' };
            string[] receiverEmails = messageDraft.ReceiverEmails.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            int foundReceivers = 0;
            // Look up receivers
            foreach (string receiverEmail in receiverEmails)
            {
                SnapUser user = _snapContext.SnapUsers.Where(s => s.Email == receiverEmail.Trim()).First();
                if (user != null)
                {
                    var message = new SnapMessage
                    {
                        OwnerId = user.ID,
                        SenderId = sendingUser.ID,
                        ReceiverEmails = messageDraft.ReceiverEmails,
                        Message = messageDraft.Message,
                        SentTimestamp = stamp,
                        Seen = false,
                        SnapMetadataId = messageDraft.SnapMetadataId
                    };
                    await _snapContext.SnapMessages.AddAsync(message);

                    foundReceivers++;
                }
                // TODO: consider what to do if one of the receivers were not found
            }
            // Have to be at least one legal receiver
            if (foundReceivers == 0)
            {
                return new BadRequestResult();
            }

            await _snapContext.SaveChangesAsync();
            return new CreatedAtRouteResult(nameof(SnapMessagesController.GetSnapMessage), new { id = sendersMessage.Id }, sendersMessage); 
        }
    }
}
