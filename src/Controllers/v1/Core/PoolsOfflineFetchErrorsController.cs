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
    // [ApiExplorerSettings(GroupName = "PoolsOfflineFetchErrors")]
    public class PoolsOfflineFetchErrorsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;

        public PoolsOfflineFetchErrorsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pool offline fetch errors.</summary>
        /// <remarks>Returns all pool offline fetch errors.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/PoolOfflineFetchError
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/pools/offlinefetcherrors")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "FetchErrors" })]
        public async Task<ActionResult<IEnumerable<PoolOfflineFetchError>>> GetPoolOfflineFetchError()
        {
            if (_context.PoolOfflineFetchError == null)
            {
                return NotFound();
            }
            return await _context.PoolOfflineFetchError.ToListAsync();
        }

        /// <summary>One pool offline fetch errors by pool id.</summary>
        /// <remarks>Returns the offline fetch errors for one pool given its unique identifier.</remarks>
        /// <param name="pool_id">Pool unique identifier</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/PoolOfflineFetchError/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/pools/{pool_id:long}/offlinefetcherrors")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "FetchErrors" })]
        public async Task<ActionResult<IEnumerable<PoolOfflineFetchError>>> GetPoolOfflineFetchError(long pool_id)
        {
            if (_context.PoolOfflineFetchError == null)
            {
                return NotFound();
            }
            var poolOfflineFetchError = await _context.PoolOfflineFetchError.Where(b => b.pool_id == pool_id).ToListAsync();

            if (poolOfflineFetchError == null)
            {
                return NotFound();
            }

            return poolOfflineFetchError;
        }
    }
}
