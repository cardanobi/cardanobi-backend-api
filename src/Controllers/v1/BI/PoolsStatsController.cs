using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApiCore.Models;
using ApiCore.DTO;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;


namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    public class PoolsStatsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<PoolsStatsController> _logger;

        public PoolsStatsController(cardanobiCoreContext context, ILogger<PoolsStatsController> logger)
        {
            _context = context;
            _logger = logger;
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
        // [EnableQuery(PageSize = 100)]
        [CustomEnableQueryAttribute("$orderby=epoch_no", PageSize = 100)]
        [HttpGet("api/bi/pools/stats/epochs/{epoch_no}")]
        [SwaggerOperation(Tags = new[] { "BI", "Epochs", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStatDTO>>> GetPoolStat(long epoch_no)
        {
            var query = _context.PoolStat
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.epoch_no == epoch_no)
                .Select(ps => new PoolStatDTO
                {
                    epoch_no = ps.epoch_no,
                    pool_hash = ps.PoolHash.view,
                    tx_count = ps.tx_count,
                    block_count = ps.block_count,
                    delegator_count = ps.delegator_count,
                    delegated_stakes = ps.delegated_stakes
                });

            var epochStats = await query.ToListAsync();

            if (epochStats == null || epochStats.Count == 0)
            {
                return NotFound();
            }

            return epochStats;
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
        // [EnableQuery(PageSize = 100)]
        [CustomEnableQueryAttribute("$orderby=epoch_no", PageSize = 100)]
        [HttpGet("api/bi/pools/{pool_hash}/stats")]
        [SwaggerOperation(Tags = new[] { "BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStatDTO>>> GetPoolStat(string pool_hash)
        {
            var query = _context.PoolStat
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.PoolHash.view == pool_hash)
                .Select(ps => new PoolStatDTO
                {
                    epoch_no = ps.epoch_no,
                    pool_hash = ps.PoolHash.view,
                    tx_count = ps.tx_count,
                    block_count = ps.block_count,
                    delegator_count = ps.delegator_count,
                    delegated_stakes = ps.delegated_stakes
                });

            var poolStats = await query.ToListAsync();

            if (poolStats == null || poolStats.Count == 0)
            {
                return NotFound();
            }

            return poolStats;
        }

        /// <summary>One pool lifetime statistics.</summary>
        /// <remarks>Pool lifetime activity statistics for a given pool.</remarks>
        /// <param name="pool_hash">The Bech32 encoding of a given pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // [EnableQuery(PageSize = 100)]
        [EnableQueryAttribute(PageSize = 1)]
        [HttpGet("api/bi/pools/{pool_hash}/stats/lifetime")]
        [SwaggerOperation(Tags = new[] { "BI", "Pools", "Lifetime Stats" })]
        public async Task<ActionResult<PoolStatLifetimeDTO>> GetPoolLifetimeStat(string pool_hash)
        {
            var poolStats = await _context.PoolStat
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.PoolHash.view == pool_hash)
                .Select(ps => new
                {
                    ps.tx_count,
                    ps.block_count,
                    ps.delegator_count,
                    ps.delegated_stakes
                })
                .ToListAsync();

            if (poolStats == null || poolStats.Count == 0)
            {
                return NotFound();
            }

            var lifetimeStats = new PoolStatLifetimeDTO
            {
                pool_hash = pool_hash,
                tx_count_lifetime = poolStats.Sum(ps => ps.tx_count),
                block_count_lifetime = poolStats.Sum(ps => ps.block_count),
                delegator_count_lifetime = poolStats.Sum(ps => ps.delegator_count),
                delegated_stakes_lifetime = poolStats.Sum(ps => ps.delegated_stakes),
                delegator_count_lifetime_avg = poolStats.Average(ps => ps.delegator_count),
                delegated_stakes_lifetime_avg = poolStats.Average(ps => ps.delegated_stakes)
            };

            return lifetimeStats;
        }
    }
}
