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


        /// <summary>
        /// Creates a snap metadata entry.
        /// </summary>
        /// <param name="command">The action command.</param>
        /// <param name="metadata">The snap metadata entry to create.</param>
        /// <param name="cancellationToken">The cancellation token used to cancel the HTTP request.</param>
        /// <returns>A 201 Created response containing the newly created snap metadata entry or a 400 Bad Request if the entry was
        /// invalid.</returns>
        /// <response code="201">Snap metadata entry was successfully created</response>
        [HttpPost]
        [ProducesResponseType(typeof(SnapMetadata), 201)]
        //[SwaggerResponse(StatusCodes.Status201Created, "The snap metadata entry was created.", typeof(SnapMetadata))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The snap metadata entry was invalid.", typeof(ModelStateDictionary))]
        public Task<IActionResult> PostSnapMetadata(
            [FromServices] IPostSnapMetadataCommand command,
            [FromBody] SnapMetadata metadata,
            CancellationToken cancellationToken) => command.ExecuteAsync(metadata);


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

    }
}
