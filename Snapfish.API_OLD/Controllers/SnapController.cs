using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Snapfish.API.Models;
using Snapfish.BL.Models;

namespace Snapfish.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnapController : Controller
    {
        private readonly SnapContext _context;

        [HttpGet("{id}")]
        public async Task<ActionResult<EchogramTransmissionPacket>> GetEchogramData(long id)
        {
            var snap = await _context.EchogramTransmissionPackets.FindAsync(id);
            if (snap == null)
            {
                return NotFound();
            }

            return snap; 
        }

        
        [HttpPost]
        public async Task<ActionResult<EchogramTransmissionPacket>> PostEchogramInfo(EchogramTransmissionPacket id)
        {
            EchogramTransmissionPacket infoClass = new EchogramTransmissionPacket();

            _context.EchogramTransmissionPackets.Add(infoClass);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetEchogramData), new { id = infoClass.Id }, infoClass);
        }
    }
}