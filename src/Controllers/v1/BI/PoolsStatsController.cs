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

namespace ApiCore.Controllers
{
    // [Route("api/core/epochs/stakes")]
    [ApiController]
    [Authorize(Policy = "bi-read")]
    // [AllowAnonymous]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "EpochsStakes")]
    public class PoolsStatsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;

        public PoolsStatsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pools statistics per epoch.</summary>
        /// <remarks>Pools activity statistics per epoch number.</remarks>
        /// <param name="epoch_no">Epoch number.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/bi/pools/stats/epochs/{epoch_no}")]
        [SwaggerOperation(Tags = new[] { "BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat(long epoch_no)
        {
            if (_context.PoolStat == null)
            {
                return NotFound();
            }
            return await _context.PoolStat.Where(b => b.epoch_no == epoch_no).OrderBy(b => b.pool_hash).ToListAsync();
        }

        /// <summary>One pool statistics per epoch.</summary>
        /// <remarks>Pool activity statistics for a given pool per epoch number.</remarks>
        /// <param name="pool_hash">The Bech32 encoding of a given pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/bi/pools/{pool_hash}/stats")]
        [SwaggerOperation(Tags = new []{"BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat(string pool_hash)
        {
          if (_context.PoolStat == null)
          {
              return NotFound();
          }
            return await _context.PoolStat.Where(b => b.pool_hash == pool_hash).ToListAsync();
        }

        // /// <summary>One pool statistics per epoch.</summary>
        // /// <remarks>Pool activity statistics for a given pool per epoch number.</remarks>
        // /// <param name="pool_hash">The Bech32 encoding of a given pool hash</param>
        // /// <param name="epoch_no_min">Epoch range lower bound</param>
        // /// <param name="epoch_no_max">Epoch range upper bound</param>
        // /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        // /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        // /// <param name="order">Prescribes in which order the delegation events are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        // /// <response code="200">OK: Successful request.</response>
        // /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        // /// <response code="401">Unauthorized: No valid API key provided.</response>
        // /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        // /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        // /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("api/bi/pools/{pool_hash}/stats")]
        // [SwaggerOperation(Tags = new[] { "BI", "Pools", "Stats" })]
        // public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat(string pool_hash, [FromQuery] long? epoch_no_min, [FromQuery] long? epoch_no_max, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        // {
        //     if (_context.PoolStat == null)
        //     {
        //         return NotFound();
        //     }

        //     string orderDir = order == null ? "desc" : order;
        //     long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1, (long)page_size));
        //     long pageNo = page_no == null ? 1 : Math.Max(1, (long)page_no);

        //     long epochNoMin = epoch_no_min == null ? 0 : Math.Max(0, (long)epoch_no_min);
        //     long epochNoMax = epoch_no_max == null ? 1000000 : Math.Max(epochNoMin, (long)epoch_no_max);

        //     if (orderDir == "desc")
        //     {
        //         return await _context.PoolStat.Where(b => b.pool_hash == pool_hash && b.epoch_no >= epochNoMin && b.epoch_no <= epochNoMax).OrderByDescending(b => b.epoch_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
        //     } 
            
        //     return await _context.PoolStat.Where(b => b.pool_hash == pool_hash && b.epoch_no >= epochNoMin && b.epoch_no <= epochNoMax).OrderBy(b => b.epoch_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
        // }
    }
}
