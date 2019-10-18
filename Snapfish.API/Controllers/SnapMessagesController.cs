using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Snapfish.API.Commands;
using Snapfish.API.Database;
using Snapfish.BL.Models;

namespace Snapfish.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnapMessagesController : ControllerBase
    {
        private readonly SnapContext _context;

        public SnapMessagesController(SnapContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Get all the snap messages for the specified user
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <response code="200">All snap messages of the specified user was retrieved.</response>
        /// <response code="404">The specified user was not found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(SnapMessage), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> GetSnapMetadatas(
            [FromServices] IGetSnapMessagesCommand command,
            CancellationToken cancellationToken,
            [FromQuery] int userId,
            [FromQuery] bool withEchogram = false,             
            [FromQuery] bool withReceivers = true) => command.ExecuteAsync(userId, withEchogram, withReceivers);


        /// <summary>
        /// Get the snap message with the specified unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the snap message entry.</param>
        /// <response code="200">The snap message with the specified unique identifier was retrieved.</response>
        /// <response code="404">No snap message with the specified identifier was found.</response>
        [HttpGet("{id}", Name = nameof(GetSnapMessage))]
        [ProducesResponseType(typeof(SnapMessage), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> GetSnapMessage(
            [FromServices] IGetSnapMessageCommand command,
            long id,
            CancellationToken cancellationToken) => command.ExecuteAsync(id);


        /// <summary>
        /// Creates a snap message for a list of recipients.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="message">The snap message to create.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <response code="201">Snap message entry was successfully created</response>
        /// <response code="400">The snap message was invalid and no entry could be created</response>
        [HttpPost]
        [ProducesResponseType(typeof(SnapMessage), 201)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public Task<IActionResult> PostSnapmessage(
            [FromServices] IPostSnapMessageCommand command,
            [FromBody] SnapMessage message,
            CancellationToken cancellationToken) => command.ExecuteAsync(message);


        /// <summary>
        /// Deletes the snap message with the specified unique identifier.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="id">The unique identifier of the snap message.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <response code="204">The snap message with the specified unique identifier was deleted.</response>
        /// <response code="404">No snap message entry with the specified identifier was found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public Task<IActionResult> Delete(
            [FromServices] IDeleteSnapMessageCommand command,
            int id,
            CancellationToken cancellationToken) => command.ExecuteAsync(id);


/*

        // GET: api/SnapMessage
        [HttpGet]
        public async Task<IEnumerable<SnapMessage>> GetSnapMessages([FromQuery] bool withEchogram = false, [FromQuery] bool withSender = true, [FromQuery] bool withReceivers = true)
        {
            if (withReceivers == false)
            {
                return await _context.SnapMessages
                                .Include(s => s.EchogramInfo)
                                .Include(s => s.Sender)
                                .ToListAsync();
            }

            if (withSender == false)
            {
                return await _context.SnapMessages
                                .Include(s => s.EchogramInfo)
                                .Include(s => s.Receivers)
                                .ToListAsync();
            }
            if (withEchogram)
            {
                return await _context.SnapMessages
                                .Include(s => s.EchogramInfo)
                                .Include(s => s.Sender)
                                .Include(s => s.Receivers)
                                .ToListAsync();

            }
            return await _context.SnapMessages.ToListAsync(); 
        }
        
        // GET: api/SnapMessage/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SnapMessage>> GetSnapMessage(long id)
        {
            var snap = await _context.SnapMessages.FindAsync(id);

            if (snap == null)
            {
                return NotFound();
            }

            return snap; 
        }
        
        
        // POST: api/SnapMessage
        [HttpPost]
        public async Task<ActionResult<SnapMessage>> PostSnapMessage(SnapMessage item)
        {
            item.ID = 0;
            item.EchogramInfo = null;
            item.SendTimestamp = DateTime.Now;

            // Look up sender
            SnapUser sendingUser = _context.SnapUsers.Where(s => s.Email == item.SenderEmail).First();
            if (sendingUser != null)
            {
                item.SenderID = sendingUser.ID;
            }

            // Look up receivers
            foreach (SnapReceiver receiver in item.Receivers)
            {
                SnapUser user = _context.SnapUsers.Where(s => s.Email == receiver.ReceiverEmail).First();
                if (user != null)
                {
                    receiver.SnapUserID = user.ID;
                }
            }

            _context.SnapMessages.Add(item);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSnapMessage), new { id = item.ID }, item);
        }
        */
        // PUT: api/SnapMessage/5
        /*
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
        */
        /*
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(long id)
        {
            var snapMessage = await _context.SnapMessages.FindAsync(id);

            if (snapMessage == null)
            {
                return NotFound();
            }

            _context.SnapMessages.Remove(snapMessage);
            await _context.SaveChangesAsync();

            return NoContent();
        }*/
    }
}
