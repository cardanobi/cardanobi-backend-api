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
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "EpochsStakes")]
    public class EpochsStakesController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<EpochsStakesController> _logger;

        public EpochsStakesController(cardanobiCoreContext context, ILogger<EpochsStakesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>One pool stake distributions.</summary>
        /// <remarks>Returns the stake distribution for one pool across all epochs given its Bech32 pool hash.</remarks>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/stakes/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake(string pool_hash)
        {
            if (_context.EpochStake == null)
            {
                return NotFound();
            }
            var epochStake = await _context.EpochStake.Where(b => b.pool_hash == pool_hash).OrderBy(b => b.epoch_stake_epoch_no).ToListAsync();

            if (epochStake == null)
            {
                return NotFound();
            }

            return epochStake;
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
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/{epoch_no}/stakes/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake(long epoch_no, string pool_hash)
        {
            if (_context.EpochStake == null)
            {
                return NotFound();
            }
            var epochStake = await _context.EpochStake.Where(b => b.epoch_stake_epoch_no == epoch_no &&  b.pool_hash == pool_hash).ToListAsync();

            if (epochStake == null)
            {
                return NotFound();
            }

            return epochStake;
        }

        /// <summary>Latest epoch and one pool stake distributions.</summary>
        /// <remarks>Returns the stake distribution for the latest epoch, and for one pool given its Bech32 pool hash.</remarks>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/latest/stakes/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetLatestEpochStake(string pool_hash)
        {
            if (
                _context.EpochStake == null ||
                _context.Epoch == null
                )
            {
                return NotFound();
            }
            long latestEpochNo = _context.Epoch.Max(b => b.no);
            _logger.LogInformation($"EpochsStakesController.GetLatestEpochStake: latestEpochNo {latestEpochNo}");

            var epochStake = await _context.EpochStake.Where(b => b.epoch_stake_epoch_no == latestEpochNo &&  b.pool_hash == pool_hash).ToListAsync();

            if (epochStake == null)
            {
                return NotFound();
            }

            return epochStake;
        }
    }
}
