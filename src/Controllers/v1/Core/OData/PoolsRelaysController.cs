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
    [Route("api/core/odata/poolsrelays")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/PoolsRelays")]
    public class PoolsRelaysController : ODataController
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
        // GET: api/PoolRelay
        [EnableQuery(PageSize = 20)]
        [HttpGet]
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
        // GET: api/PoolRelay/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("{update_id}")]
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
    }
}
