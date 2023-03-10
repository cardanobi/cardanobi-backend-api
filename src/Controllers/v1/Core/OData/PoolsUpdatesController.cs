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
    [Route("api/core/odata/poolsupdates")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/PoolsUpdates")]
    public class PoolsUpdatesController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public PoolsUpdatesController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All on-chain pool updates.</summary>
        /// <remarks>Returns all on-chain pool updates.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolUpdate
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Updates" })]
        public async Task<ActionResult<IEnumerable<PoolUpdate>>> GetPoolUpdate()
        {
          if (_context.PoolUpdate == null)
          {
              return NotFound();
          }
            return await _context.PoolUpdate.ToListAsync();
        }

        /// <summary>One pool on-chain updates.</summary>
        /// <remarks>Returns the on-chain updates for one pool given its unique identifier.</remarks>
        /// <param name="pool_id">Pool unique identifier</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolUpdate/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("{pool_id:long}")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Updates" })]
        public async Task<ActionResult<IEnumerable<PoolUpdate>>> GetPoolUpdate(long pool_id)
        {
          if (_context.PoolUpdate == null)
          {
              return NotFound();
          }
            var poolUpdate = await _context.PoolUpdate.Where(b => b.hash_id == pool_id).ToListAsync();

            if (poolUpdate == null)
            {
                return NotFound();
            }

            return poolUpdate;
        }

        /// <summary>One pool on-chain updates.</summary>
        /// <remarks>Returns the on-chain updates for one pool given its VRF key hash.</remarks>
        /// <param name="vrf_key_hash">The pool VRF key in HEX format.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // // GET: api/PoolUpdate/5
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("{vrf_key_hash:length(64)}")]
        // [SwaggerOperation(Tags = new []{"Core", "Pools", "Updates" })]
        // public async Task<ActionResult<IEnumerable<PoolUpdate>>> GetPoolUpdate(string vrf_key_hash)
        // {
        //     if (_context.PoolUpdate == null)
        //     {
        //         return NotFound();
        //     }
        //     try
        //     {
        //         byte[] _res = Convert.FromHexString(vrf_key_hash);
        //     }
        //     catch (Exception e)
        //     {
        //         return NotFound();
        //     }
        //     var poolUpdate = await _context.PoolUpdate.Where(b => b.vrf_key_hash == Convert.FromHexString(vrf_key_hash)).ToListAsync();

        //     if (poolUpdate == null)
        //     {
        //         return NotFound();
        //     }

        //     return poolUpdate;
        // }
    }
}
