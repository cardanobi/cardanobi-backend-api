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
    [Route("api/bi/odata/addressesstats")]
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    public class AddressesStatsController : ODataController
    {
        private readonly cardanobiBIContext _context;

        public AddressesStatsController(cardanobiBIContext context)
        {
            _context = context;
        }

        /// <summary>All stake addresses stats per epoch.</summary>
        /// <remarks>Returns stake addresses statistics per epoch.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/AddressStat
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"BI", "Addresses", "Stats" })]
        public async Task<ActionResult<IEnumerable<AddressStat>>> GetAddressStat()
        {
            if (_context.AddressStat == null)
            {
                return NotFound();
            }
            return await _context.AddressStat.OrderBy(b => b.epoch_no).ThenBy(b => b.stake_address).ToListAsync();
        }
    }
}
