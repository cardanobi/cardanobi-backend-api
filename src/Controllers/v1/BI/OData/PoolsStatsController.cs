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
    [Route("api/bi/odata/poolsstats")]
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/EpochsStakes")]
    public class PoolsStatsController : ODataController
    {
        private readonly cardanobiBIContext _context;

        public PoolsStatsController(cardanobiBIContext context)
        {
            _context = context;
        }

        /// <summary>All pools statistics per epoch.</summary>
        /// <remarks>Pools activity statistics per epoch number.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"BI", "Pools", "Stats" })]
        public async Task<ActionResult<IEnumerable<PoolStat>>> GetPoolStat()
        {
          if (_context.PoolStat == null)
          {
              return NotFound();
          }
            return await _context.PoolStat.OrderBy(b => b.epoch_no).ThenBy(b => b.pool_hash).ToListAsync();
        }
    }
}
