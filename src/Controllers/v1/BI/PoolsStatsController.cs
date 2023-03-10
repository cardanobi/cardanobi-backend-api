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
        private readonly cardanobiBIContext _context;

        public PoolsStatsController(cardanobiBIContext context)
        {
            _context = context;
        }

        /// <summary>All pools statistics per epoch.</summary>
        /// <remarks>Pools activity statistics per epoch number.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/bi/pools/stats")]
        [SwaggerOperation(Tags = new []{"BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat()
        {
          if (_context.PoolStat == null)
          {
              return NotFound();
          }
            return await _context.PoolStat.OrderBy(b => b.epoch_no).ThenBy(b => b.pool_hash).ToListAsync();
        }

        /// <summary>One pool statistics per epoch.</summary>
        /// <remarks>Pool activity statistics for a given pool per epoch number.</remarks>
        /// <param name="pool_hash">The Bech32 encoding of a given pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/bi/pools/{pool_hash}/stats")]
        [SwaggerOperation(Tags = new []{"BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat(string? pool_hash)
        {
          if (_context.PoolStat == null)
          {
              return NotFound();
          }
            return await _context.PoolStat.Where(b => b.pool_hash == pool_hash).OrderBy(b => b.epoch_no).ToListAsync();
        }
    }
}
