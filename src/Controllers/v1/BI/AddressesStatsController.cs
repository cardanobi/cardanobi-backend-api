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
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    public class AddressesStatsController : ControllerBase
    {
        private readonly cardanobiBIContext _context;

        public AddressesStatsController(cardanobiBIContext context)
        {
            _context = context;
        }

        // /// <summary>All stake addresses stats per epoch.</summary>
        // /// <remarks>Returns stake addresses statistics per epoch.</remarks>
        // // /// <response code="200">OK: Successful request.</response>
        // // /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        // // /// <response code="401">Unauthorized: No valid API key provided.</response>
        // // /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        // // /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        // // /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // // /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // [EnableQuery(PageSize = 20)]
        // [HttpGet("api/bi/addresses/stats")]
        // [SwaggerOperation(Tags = new []{"BI", "Addresses", "Stats" })]
        // public async Task<ActionResult<IEnumerable<AddressStat>>> GetAddressStat()
        // {
        //     if (_context.AddressStat == null)
        //     {
        //         return NotFound();
        //     }
        //     return await _context.AddressStat.OrderBy(b => b.epoch_no).ThenBy(b => b.stake_address).ToListAsync();
        // }

        /// <summary>One stake address stats per epoch.</summary>
        /// <remarks>Returns statistics for one given stake address and for all epochs.</remarks>
        /// <param name="stake_address">Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/bi/addresses/{stake_address}/stats")]
        [SwaggerOperation(Tags = new []{"BI", "Addresses", "Stats" })]
        public async Task<ActionResult<IEnumerable<AddressStat>>> GetAddressStat(string stake_address)
        {
            if (_context.AddressStat == null)
            {
                return NotFound();
            }
            return await _context.AddressStat.Where(b => b.stake_address == stake_address).OrderBy(b => b.epoch_no).ToListAsync();
        }

        /// <summary>All stake addresses stats for one epoch.</summary>
        /// <remarks>Returns all stake addresses statistics for one given epoch.</remarks>
        /// <param name="epoch_no">Epoch number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/bi/addresses/stats/epochs/{epoch_no}")]
        [SwaggerOperation(Tags = new []{"BI", "Addresses", "Stats" })]
        public async Task<ActionResult<IEnumerable<AddressStat>>> GetAddressStat(long epoch_no)
        {
            if (_context.AddressStat == null)
            {
                return NotFound();
            }
            return await _context.AddressStat.Where(b => b.epoch_no == epoch_no).OrderBy(b => b.stake_address).ToListAsync();
        }
    }
}
