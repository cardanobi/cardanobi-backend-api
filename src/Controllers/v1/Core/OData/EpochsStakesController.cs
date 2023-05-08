using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApiCore.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCore.Controllers.Odata
{
    [Route("api/core/odata/epochsstakes")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/EpochsStakes")]
    public class EpochsStakesViewsController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public EpochsStakesViewsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>One epoch and one pool stake distributions.</summary>
        /// <remarks>Returns the stake distribution for one epoch given its number, and for one pool given its Bech32 pool hash.</remarks>
        /// <param name="epoch_no">Epoch number</param>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<EpochStakeView>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<EpochStakeView>>> GetEpochStakeView([FromQuery] long? epoch_no, [FromQuery] string? pool_hash)
        {
            if (_context.EpochStakeView == null)
            {
                return NotFound();
            }
            if (epoch_no is null || pool_hash is null) return BadRequest("epoch_no and pool_hash should not be null!");

            var EpochStakeView = await _context.EpochStakeView.Where(b => b.epoch_stake_epoch_no == epoch_no &&  b.pool_hash == pool_hash).ToListAsync();

            if (EpochStakeView == null)
            {
                return NotFound();
            }

            return EpochStakeView;
        }
    }
}
