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
    [Route("api/core/odata/poolsofflinedata")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/PoolsOfflineData")]
    public class PoolsOfflineDataController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public PoolsOfflineDataController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pool offline data.</summary>
        /// <remarks>Returns all pool offline (ie not on chain) data.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/PoolOfflineData
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "OfflineData" })]
        public async Task<ActionResult<IEnumerable<PoolOfflineData>>> GetPoolOfflineData()
        {
            if (_context.PoolOfflineData == null)
            {
                return NotFound();
            }
            return await _context.PoolOfflineData.ToListAsync();
        }

        /// <summary>One pool offline data by pool id.</summary>
        /// <remarks>Returns the offline (ie not on chain) data for one pool given its unique identifier.</remarks>
        /// <param name="pool_id">Pool unique identifier</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/PoolOfflineData/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("{pool_id:long}")]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "OfflineData" })]
        public async Task<ActionResult<IEnumerable<PoolOfflineData>>> GetPoolOfflineData(long pool_id)
        {
            if (_context.PoolOfflineData == null)
            {
                return NotFound();
            }
            var poolOfflineData = await _context.PoolOfflineData.Where(b => b.pool_id == pool_id).ToListAsync();

            if (poolOfflineData == null)
            {
                return NotFound();
            }

            return poolOfflineData;
        }

        /// <summary>One pool offline data by pool ticker.</summary>
        /// <remarks>Returns the offline (ie not on chain) data for one pool given its ticker.</remarks>
        /// <param name="ticker">Pool ticker</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // // GET: api/PoolOfflineData/5
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("{ticker:alpha:maxlength(5)}")]
        // [SwaggerOperation(Tags = new []{"Core", "Pools", "OfflineData" })]
        // public async Task<ActionResult<IEnumerable<PoolOfflineData>>> GetPoolOfflineDataFromTicker(string ticker)
        // {
        //     if (_context.PoolOfflineData == null)
        //     {
        //         return NotFound();
        //     }
        //     var poolOfflineData = await _context.PoolOfflineData.Where(b => b.ticker_name.Equals(ticker)).ToListAsync();

        //     if (poolOfflineData == null)
        //     {
        //         return NotFound();
        //     }

        //     return poolOfflineData;
        // }

        /// <summary>One pool offline data by pool metadata hash.</summary>
        /// <remarks>Returns the offline (ie not on chain) data for one pool given its metadata hash.</remarks>
        /// <param name="meta_hash">Pool metadata hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // // GET: api/PoolOfflineData/5
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("{pool_hash:length(64)}")]
        // [SwaggerOperation(Tags = new []{"Core", "Pools", "OfflineData" })]
        // public async Task<ActionResult<IEnumerable<PoolOfflineData>>> GetPoolOfflineDataFromHash(string pool_hash)
        // {
        //     if (_context.PoolOfflineData == null)
        //     {
        //         return NotFound();
        //     }
        //     try
        //     {
        //         byte[] _res = Convert.FromHexString(pool_hash);
        //     }
        //     catch (Exception e)
        //     {
        //         return NotFound();
        //     }
        //     var poolOfflineData = await _context.PoolOfflineData.Where(b => b.hash == Convert.FromHexString(pool_hash)).ToListAsync();

        //     if (poolOfflineData == null)
        //     {
        //         return NotFound();
        //     }

        //     return poolOfflineData;
        // }
    }
}
