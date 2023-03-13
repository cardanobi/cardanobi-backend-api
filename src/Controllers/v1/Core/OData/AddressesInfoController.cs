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
    [Route("api/core/odata/addressesinfo")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    public class AddressesInfoController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public AddressesInfoController(cardanobiCoreContext context)
        {
            _context = context;
        }


        /// <summary>All addresses information.</summary>
        /// <remarks>Returns useful information for all addresses.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/AddressInfo
        [EnableQuery(PageSize = 20)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Addresses", "Info" })]
        public async Task<ActionResult<IEnumerable<AddressInfo>>> GetAddressInfo()
        {
          if (_context.AddressInfo == null)
          {
              return NotFound();
          }
            return await _context.AddressInfo.OrderBy(b => b.address).ToListAsync();
        }

        /// <summary>One address information.</summary>
        /// <remarks>Returns useful information for one given payment address or all payment addresses linked to a given stake address.</remarks>
        /// <param name="address">A payment address or a stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/AddressInfo
        [EnableQuery(PageSize = 20)]
        [HttpGet(template: "{address}")]
        [SwaggerOperation(Tags = new []{"Core", "Addresses", "Info" })]
        public async Task<ActionResult<IEnumerable<AddressInfo>>> GetAddressInfo(string? address)
        {
            if (_context.AddressInfo == null)
            {
                return NotFound();
            }

            // Log.Information($"AddressesInfoController.GetAddressInfo: {address} ,substr: {address.Substring(0, 5)}");

            if(address.Substring(0,5).Equals("stake")) 
                return await _context.AddressInfo.Where(b => b.stake_address == address).OrderBy(b => b.address).ToListAsync();
              else
                return await _context.AddressInfo.Where(b => b.address == address).ToListAsync();
        }
    }
}
