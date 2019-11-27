using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Snapfish.BL.Models;
using Snapfish.API.Database;
using Snapfish.API.Commands;
using System.Threading;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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


        /// <summary>
        /// Get the snap with the specified unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the snap.</param>
        /// <response code="200">The snap with the specified unique identifier was retrieved.</response>
        /// <response code="404">No snap with the specified identifier was found.</response>
        [HttpGet("{id}", Name = nameof(GetSnap))]
        [ProducesResponseType(typeof(SnapPacket), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> GetSnap(
            [FromServices] IGetSnapCommand command,
            long id,
            CancellationToken cancellationToken) => command.ExecuteAsync(id);

/*
        
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
*/        

        /// <summary>
        /// Creates a new snap with a snap metadata entry.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="snap">The snap packet containing the snap and metadata for the entry to create.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <response code="201">Snap and snap metadata entry was successfully created</response>
        /// <response code="400">The snap packet was invalid and no snap or snap metadata entry could be created</response>
        [HttpPost]
        [RequestSizeLimit(1000000)]
        [ProducesResponseType(typeof(Snap), 201)]
        [ProducesResponseType(typeof(ModelStateDictionary), 400)]
        public Task<IActionResult> PostSnap(
            [FromServices] IPostSnapCommand command,
            [FromBody] SnapPacket snap,
            CancellationToken cancellationToken) => command.ExecuteAsync(snap);

        /*
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
        }*/
    }
}