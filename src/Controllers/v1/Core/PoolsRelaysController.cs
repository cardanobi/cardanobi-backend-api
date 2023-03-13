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
    [ApiController]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "PoolsRelays")]
    public class PoolsRelaysController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;

        public PoolsRelaysController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All relays.</summary>
        /// <remarks>Returns all pool relays.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolRelay
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/pools/relays/updates")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Relays" })]
        public async Task<ActionResult<IEnumerable<PoolRelay>>> GetPoolRelay()
        {
          if (_context.PoolRelay == null)
          {
              return NotFound();
          }
            return await _context.PoolRelay.ToListAsync();
        }

        /// <summary>One pool relays by pool update unique identifier.</summary>
        /// <remarks>Returns the relays for one pool given a pool update unique identifier.</remarks>
        /// <param name="update_id">The pool update unique identifier</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolRelay/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/pools/relays/updates/{update_id}")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Relays" })]
        public async Task<ActionResult<IEnumerable<PoolRelay>>> GetPoolRelay(long update_id)
        {
          if (_context.PoolRelay == null)
          {
              return NotFound();
          }
            var poolRelay = await _context.PoolRelay.Where(b => b.update_id == update_id).ToListAsync();

            if (poolRelay == null)
            {
                return NotFound();
            }

            return poolRelay;
        }

        /// <summary>One pool relays by VRF key hash.</summary>
        /// <remarks>Returns the relays for one pool given its VRF key hash.</remarks>
        /// <param name="vrf_key_hash">The pool VRF key in HEX format.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolRelay/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/pools/{vrf_key_hash:length(64)}/relays/updates")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Relays" })]
        public async Task<ActionResult<IEnumerable<PoolRelay>>> GetPoolRelayFromVrfKeyHash(string vrf_key_hash)
        {
            if (_context.PoolRelay == null)
            {
                return NotFound();
            }
            var query = (
                    from pr in _context.PoolRelay
                    join pu in _context.PoolUpdate on pr.update_id equals pu.id
                    where pu.vrf_key_hash == Convert.FromHexString(vrf_key_hash)
                    select pr
            );

            var poolRelay = await query.ToListAsync().ConfigureAwait(false); 

            if (poolRelay == null)
            {
                return NotFound();
            }

            return Ok(poolRelay);
        }


    }
}
