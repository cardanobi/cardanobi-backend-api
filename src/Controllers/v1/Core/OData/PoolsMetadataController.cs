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
    [Route("api/core/odata/poolsmetadata")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/PoolsMetadata")]
    public class PoolsMetadataController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public PoolsMetadataController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pool metadata.</summary>
        /// <remarks>Returns all on-chain references to off-chain pool metadata.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolMetadata
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Metadata" })]
        public async Task<ActionResult<IEnumerable<PoolMetadata>>> GetPoolMetadata()
        {
            if (_context.PoolMetadata == null)
            {
                return NotFound();
            }
            return await _context.PoolMetadata.ToListAsync();
        }

        /// <summary>One pool metadata by pool id.</summary>
        /// <remarks>Returns the on-chain references to off-chain pool metadata for one pool given its unique identifier.</remarks>
        /// <param name="pool_id">Pool unique identifier</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/PoolMetadata/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("{pool_id:long}")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Metadata" })]
        public async Task<ActionResult<IEnumerable<PoolMetadata>>> GetPoolMetadata(long pool_id)
        {
            if (_context.PoolMetadata == null)
            {
                return NotFound();
            }
            var poolMetadata = await _context.PoolMetadata.Where(b => b.pool_id == pool_id).ToListAsync();

            if (poolMetadata == null)
            {
                return NotFound();
            }

            return poolMetadata;
        }

        /// <summary>One pool metadata by pool metadata hash.</summary>
        /// <remarks>Returns the on-chain references to off-chain pool metadata for one pool given its metadata hash.</remarks>
        /// <param name="meta_hash">Pool metadata hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // // GET: api/PoolMetadata/5
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("{meta_hash:alpha}")]
        // [SwaggerOperation(Tags = new []{"Core", "Pools", "Metadata" })]
        // public async Task<ActionResult<IEnumerable<PoolMetadata>>> GetPoolMetadata(string meta_hash)
        // {
        //     if (_context.PoolMetadata == null)
        //     {
        //         return NotFound();
        //     }
        //     try
        //     {
        //         byte[] _res = Convert.FromHexString(meta_hash);
        //     }
        //     catch (Exception e)
        //     {
        //         return NotFound();
        //     }

        //     var poolMetadata = await _context.PoolMetadata.Where(b => b.hash == Convert.FromHexString(meta_hash)).ToListAsync();

        //     if (poolMetadata == null)
        //     {
        //         return NotFound();
        //     }

        //     return poolMetadata;
        // }
    }
}
