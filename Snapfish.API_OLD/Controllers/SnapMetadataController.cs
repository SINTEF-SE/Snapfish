using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapfish.API.Models;
using Snapfish.BL.Models;

namespace Snapfish.API_OLD.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnapMetadataController : ControllerBase
    {

        private readonly SnapContext _context;

        public SnapMetadataController(SnapContext context)
        {
            _context = context;
            
            if (_context.SnapMetadatas.Any()) return;
            
            _context.SnapMetadatas.Add(new SnapMetadata { Source = "EK80", Latitude = "432233", Longitude= "123456", Timestamp = DateTime.Now });
            _context.SaveChanges();

        }

        // GET: api/EchogramInfo
        [HttpGet]
        public async Task<IEnumerable<SnapMetadata>> GetEchogramInfos()
        {
            return await _context.SnapMetadatas
                            //TODO: Check   .Include(e => e.OwnerId)
                            .ToListAsync ();
        }

        // GET: api/EchogramInfo/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<SnapMetadata>> GetEchogramInfo(long id)
        {
            var snap = await _context.SnapMetadatas.FindAsync(id);

            if (snap == null)
            {
                return NotFound();
            }

            return snap;
        }

        // POST: api/SnapMetadata
        [HttpPost]
        public async Task<ActionResult<SnapMetadata>> PostSnapMetadata([FromBody] SnapMetadata metadata)
        {
            _context.SnapMetadatas.Add(metadata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEchogramInfo), new { id = metadata.Id }, metadata);
        }
/*        
        //TODO: REMOVE ME ONLY FOR DEMO, LETS DO THIS SAFELY TO ENSURE NO FUCK UPS
        [HttpPost]
        public async Task<ActionResult<int>> PostEchogramInfo(int id)
        {
            System.Console.WriteLine("post received in echograminfoscontroller");
            EchogramInfo infoClass = new EchogramInfo();
        //    switch (id)
//            {
  //              case 1:
                    //infoClass.ID = 15;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "https://10.218.87.81:5006/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    infoClass.OwnerID = 2;
                    break;
                case 2:
                    //infoClass.ID = 16;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "https://10.218.87.81:5006/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    break;
                case 3:
                    //infoClass.ID = 17;
                    infoClass.Latitude = "632468";
                    infoClass.Longitude = "116306";
                    infoClass.Source = "Kanonbåtens EK80";
                    infoClass.EchogramUrl = "https://10.218.87.81:5006/test/index.html";
                    infoClass.Timestamp = DateTime.Now;
                    infoClass.Biomass = "341";
                    break;
            }

            _context.EchogramInfos.Add(infoClass);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEchogramInfo), new { id = infoClass.ID }, infoClass);
        }
*/

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
            var echogramInfo = await _context.SnapMetadatas.FindAsync(id);

            if (echogramInfo == null)
            {
                return NotFound();
            }

            _context.SnapMetadatas.Remove(echogramInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
