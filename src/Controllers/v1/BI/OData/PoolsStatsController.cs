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
    [Route("api/bi/odata/poolsstats")]
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/EpochsStakes")]
    public class PoolsStatsController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public PoolsStatsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pools statistics per epoch.</summary>
        /// <remarks>Pools activity statistics per epoch number.</remarks>
        /// <param name="epoch_no">Epoch number</param>
        /// <param name="pool_hash">The Bech32 encoding of a given pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new[] { "BI", "Pools", "Stats" })]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PoolStat>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat([FromQuery] long? epoch_no, [FromQuery] string? pool_hash)
        {
            if (_context.PoolStat == null)
            {
                return NotFound();
            }
            if (epoch_no is null && pool_hash is null) return BadRequest("epoch_no or pool_hash should not be null!");

            if (epoch_no is not null && pool_hash is null)
                return await _context.PoolStat.Where(b => b.epoch_no == epoch_no).OrderBy(b => b.pool_hash).ToListAsync();
            else if (epoch_no is null && pool_hash is not null)
                return await _context.PoolStat.Where(b => b.pool_hash == pool_hash).OrderBy(b => b.epoch_no).ToListAsync();

            return await _context.PoolStat.Where(b => b.epoch_no == epoch_no).Where(b => b.pool_hash == pool_hash).ToListAsync();
        }
    }
}
