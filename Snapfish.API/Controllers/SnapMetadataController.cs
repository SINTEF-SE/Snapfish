using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snapfish.BL.Models;
using Snapfish.API.Database;
using Snapfish.API.Commands;
using System.Threading;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Snapfish.API.Controllers
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

            _context.SnapMetadatas.Add(new SnapMetadata { Source = "EK80", Latitude = "432233", Longitude = "123456", Timestamp = DateTime.Now });
            _context.SaveChanges();

        }


        /// <summary>
        /// Get all the snap metadata entries for the specified owner
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner.</param>
        [HttpGet]
        public Task<IActionResult> GetSnapMetadatas(
            [FromServices] IGetSnapMetadatasCommand command,
            [FromQuery] int ownerId,
            CancellationToken cancellationToken) => command.ExecuteAsync(ownerId);

        /*
        // GET: api/EchogramInfo
        [HttpGet]
        public async Task<IEnumerable<SnapMetadata>> GetEchogramInfos()
        {
            return await _context.SnapMetadatas
                            //TODO: Check   .Include(e => e.OwnerId)
                            .ToListAsync ();
        }*/

        /// <summary>
        /// Get the snap metadata entry with the specified unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the snap metadata entry.</param>
        /// <response code="200">The snap metadata with the specified unique identifier was retrieved.</response>
        /// <response code="404">No snap metadata entry with the specified identifier was found.</response>
        [HttpGet("{id}", Name=nameof(GetSnapMetadata))]
        [ProducesResponseType(typeof(SnapMetadata), 200)]
        [ProducesResponseType(404)]
        public Task<IActionResult> GetSnapMetadata(
            [FromServices] IGetSnapMetadataCommand command,
            long id,
            CancellationToken cancellationToken) => command.ExecuteAsync(id);


        /*
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
        */



        /// <summary>
        /// Creates a snap metadata entry.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="metadata">The snap metadata entry to create.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <returns>A 201 Created response containing the newly created snap metadata entry or a 400 Bad Request if the entry was
        /// invalid.</returns>
        /// <response code="201">Snap metadata entry was created</response>
        [HttpPost]
        [ProducesResponseType(typeof(SnapMetadata), 201)]
        //[SwaggerResponse(StatusCodes.Status201Created, "The snap metadata entry was created.", typeof(SnapMetadata))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The snap metadata entry was invalid.", typeof(ModelStateDictionary))]
        public Task<IActionResult> PostSnapMetadata(
            [FromServices] IPostSnapMetadataCommand command,
            [FromBody] SnapMetadata metadata,
            CancellationToken cancellationToken) => command.ExecuteAsync(metadata);

        /*
        // POST: api/SnapMetadata
        [HttpPost]
        public async Task<ActionResult<SnapMetadata>> PostSnapMetadata([FromBody] SnapMetadata metadata)
        {
            _context.SnapMetadatas.Add(metadata);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSnapMetadata), new { id = metadata.Id }, metadata);
        }*/
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


        /// <summary>
        /// Deletes the snap metadata with the specified unique identifier.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="id">The unique identifier of the snap metadata.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <returns>A 204 No Content response if the metadata was deleted or a 404 Not Found if metadata with the specified
        /// unique identifier was not found.</returns>
        [HttpDelete("{id}")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The snap metadata with the specified unique identifier was deleted.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "No snap metadata with the specified unique identifier was found.")]
        public Task<IActionResult> Delete(
            [FromServices] IDeleteSnapMetadataCommand command,
            int id,
            CancellationToken cancellationToken) => command.ExecuteAsync(id);

/*
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
        }*/
    }
}
