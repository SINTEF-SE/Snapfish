using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapfish.API.Models;

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
/*            if (_context.SnapMessages.Count() == 0)
            {
                _context.SnapMessages.Add(new SnapMessage { Title = "First snap!", Sender = "Ola", SendTimestamp = DateTime.Now });
                _context.SaveChanges();
            }*/

        }
        
        // GET: api/SnapMessage
        [HttpGet]
        public async Task<IEnumerable<SnapMessage>> GetSnapMessages([FromQuery] bool withEchogram=false)
        {
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
