using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapfish.BL.Models;
using Snapfish.API.Database;

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

        // PUT: api/SnapMessage/5
        /*
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }
        */

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
        }
    }
}
