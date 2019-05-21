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
    public class EchogramInfosController : ControllerBase
    {

        private readonly SnapContext _context;

        public EchogramInfosController(SnapContext context)
        {
            _context = context;
            if (_context.EchogramInfos.Count() == 0)
            {
                _context.EchogramInfos.Add(new EchogramInfo { Source = "EK80", Latitude = "432233", Longitude= "123456", Timestamp = DateTime.Now });
                _context.SaveChanges();
            }

        }

        // GET: api/EchogramInfo
        [HttpGet]
        public async Task<IEnumerable<EchogramInfo>> GetEchogramInfos()
        {
            return await _context.EchogramInfos
                            .Include(e => e.Owner)
                            .ToListAsync ();
        }

        // GET: api/EchogramInfo/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<EchogramInfo>> GetEchogramInfo(long id)
        {
            var snap = await _context.EchogramInfos.FindAsync(id);

            if (snap == null)
            {
                return NotFound();
            }

            return snap;
        }

        // POST: api/EchogramInfo
        [HttpPost]
        public async Task<ActionResult<EchogramInfo>> PostEchogramInfo(EchogramInfo item)
        {
            _context.EchogramInfos.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEchogramInfo), new { id = item.ID }, item);
        }

        /*
        // PUT: api/EchogramInfo/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEchogramInfo(long id)
        {
            var echogramInfo = await _context.EchogramInfos.FindAsync(id);

            if (echogramInfo == null)
            {
                return NotFound();
            }

            _context.EchogramInfos.Remove(echogramInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
