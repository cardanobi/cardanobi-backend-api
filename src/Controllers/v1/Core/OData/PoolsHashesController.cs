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
    [Route("api/core/odata/poolshashes")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/PoolsHashes")]
    public class PoolsHashesController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public PoolsHashesController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All pool key hash.</summary>
        /// <remarks>Returns every unique pool key hash.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/PoolHash
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Pools", "Hashes" })]
        public async Task<ActionResult<IEnumerable<PoolHash>>> GetPoolHash()
        {
          if (_context.PoolHash == null)
          {
              return NotFound();
          }
            return await _context.PoolHash.ToListAsync();
        }
    }
}
