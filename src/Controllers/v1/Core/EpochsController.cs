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
    [Route("api/core/epochs")]
    [ApiController]
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "Epochs")]
    public class EpochsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;

        public EpochsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All epochs.</summary>
        /// <remarks>Returns all epoch entities.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Epoch
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Epochs"})]
        public async Task<ActionResult<IEnumerable<Epoch>>> GetEpoch()
        {
            if (_context.Epoch == null)
            {
                return NotFound();
            }
            // return await _context.Epoch.ToListAsync();
            return await _context.Epoch.OrderBy(b => b.id).ToListAsync();
        }

        /// <summary>One epoch by number.</summary>
        /// <remarks>Returns one specific epoch given its number.</remarks>
        /// <param name="no">Epoch number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Epoch/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("{no}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs"})]
        public async Task<ActionResult<Epoch>> GetEpoch(int no)
        {
            if (_context.Epoch == null)
            {
                return NotFound();
            }
            var epoch = await _context.Epoch.Where(b => b.no == no).SingleOrDefaultAsync();

            if (epoch == null)
            {
                return NotFound();
            }

            return epoch;
        }
    }
}
