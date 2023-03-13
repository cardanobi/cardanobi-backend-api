using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ApiCore.Models;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Swashbuckle.AspNetCore.Annotations;

namespace ApiCore.Controllers.Odata
{
    [Route("api/core/odata/epochsparams")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/EpochsParams")]
    public class EpochsParamsController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public EpochsParamsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>All epoch params.</summary>
        /// <remarks>Returns the parameters for all epoch.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochParam
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Parameters"})]
        public async Task<ActionResult<IEnumerable<EpochParam>>> GetEpochParam()
        {
            if (_context.EpochParam == null)
            {
                return NotFound();
            }
            return await _context.EpochParam.OrderBy(b => b.id).ToListAsync();
        }

        /// <summary>One epoch params by number.</summary>
        /// <remarks>Returns the parameters of one specific epoch given its number.</remarks>
        /// <param name="no">Epoch number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/EpochParam/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("{no}")]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Parameters"})]
        public async Task<ActionResult<EpochParam>> GetEpochParam(int no)
        {
            if (_context.EpochParam == null)
            {
                return NotFound();
            }
            var epochParam = await _context.EpochParam.Where(b => b.epoch_no == no).SingleOrDefaultAsync();

            if (epochParam == null)
            {
                return NotFound();
            }

            return epochParam;
        }
    }
}
