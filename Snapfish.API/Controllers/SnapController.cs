using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Snapfish.API.Models;
using Snapfish.BL.Models;

namespace Snapfish.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnapController : Controller
    {
        private readonly SnapContext _context;

        public SnapController(SnapContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetSnap(long id)
        {
            var snap = await _context.Snaps.FindAsync(id);
            if (snap == null)
            {
                return NotFound();
            }

            return snap.SnapPacketJson;
        }
        
        [HttpPost]
        public async Task<ActionResult<string>> PostSnap([FromBody] SnapPacket snapPacket)
        {
            var snap = new Snap
            {
                SnapPacketJson = JsonConvert.SerializeObject(snapPacket)
            };
            
            _context.Snaps.Add(snap);

            var metadata = new SnapMetadata
            {
                SnapId = snap.Id,
                OwnerId = snapPacket.OwnerId,
                Timestamp = snapPacket.Timestamp,
                Latitude = snapPacket.Latitude,
                Longitude = snapPacket.Longitude
            };

            _context.SnapMetadatas.AddAsync(metadata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSnap), new { id = snap.Id }, snapPacket);
        }
    }
}