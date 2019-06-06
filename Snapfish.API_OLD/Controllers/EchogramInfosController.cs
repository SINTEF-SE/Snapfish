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
        
        //TODO: REMOVE ME ONLY FOR DEMO, LETS DO THIS SAFELY TO ENSURE NO FUCK UPS
        [HttpPost]
        public async Task<ActionResult<int>> PostEchogramInfo(int id)
        {
            EchogramInfo infoClass = new EchogramInfo();
            switch (id)
            {
                case 1:
                    infoClass.ID = 15;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "IP.TIL.ERLENDS.MASKIN/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    break;
                case 2:
                    infoClass.ID = 16;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "IP.TIL.ERLENDS.MASKIN/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    break;
                case 3:
                    infoClass.ID = 17;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "IP.TIL.ERLENDS.MASKIN/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    break;
            }

            _context.EchogramInfos.Add(infoClass);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEchogramInfo), new { id = infoClass.ID }, infoClass);
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
