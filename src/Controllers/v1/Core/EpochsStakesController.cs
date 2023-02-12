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

        public EpochsStakesController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All epoch stake distributions.</summary>
        /// <remarks>Returns stake distributions for all epochs and all pools.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochStake
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("api/core/epochs/stakes")]
        // [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        // public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake()
        // {
        //   if (_context.EpochStake == null)
        //   {
        //       return NotFound();
        //   }
        //     return await _context.EpochStake.ToListAsync();
        // }

        /// <summary>One epoch stake distributions.</summary>
        /// <remarks>Returns the stake distribution for one epoch given its number.</remarks>
        /// <param name="no">Epoch number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/{no}/stakes")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake(long? no)
        {
          if (_context.EpochStake == null)
          {
              return NotFound();
          }
            var epochStake = await _context.EpochStake.Where(b => b.epoch_stake_epoch_no == no).ToListAsync();

            if (epochStake == null)
            {
                return NotFound();
            }

            return epochStake;
        }

        /// <summary>One pool stake distributions.</summary>
        /// <remarks>Returns the stake distribution for one pool across all epochs given its Bech32 pool hash.</remarks>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/stakes/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake(string? pool_hash)
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
        /// <param name="no">Epoch number</param>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochStake/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/epochs/{no}/stakes/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        public async Task<ActionResult<IEnumerable<EpochStake>>> GetEpochStake(long? no, string? pool_hash)
        {
            if (_context.EpochStake == null)
            {
                return NotFound();
            }
            var epochStake = await _context.EpochStake.Where(b => b.epoch_stake_epoch_no == no &&  b.pool_hash == pool_hash).ToListAsync();

            if (epochStake == null)
            {
                return NotFound();
            }

            return epochStake;
        }
    }
}
