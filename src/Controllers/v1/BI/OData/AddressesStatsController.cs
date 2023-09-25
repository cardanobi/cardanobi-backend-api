using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApiCore.Models;
using ApiCore.DTO;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using System.Text;

namespace ApiCore.Controllers.Odata
{
    [Route("api/bi/odata/addressesstats")]
    [Authorize(Policy = "bi-read")]
    [Produces("application/json")]
    public class AddressesStatsController : ODataController
    {
        private readonly cardanobiCoreContext _context;

        public AddressesStatsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>One stake address stats per epoch.</summary>
        /// <remarks>Returns statistics for one given stake address and for all epochs.</remarks>
        /// <param name="address">An Enterprise address, a Payment address or a Staking address (e.g. an account)</param>
        /// <param name="epoch_no_min">Epoch range lower bound</param>
        /// <param name="epoch_no_max">Epoch range upper bound</param>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order the delegation events are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 20)]
        // [HttpGet(template: "{address}")]
        [HttpGet]
        [SwaggerOperation(Tags = new[] { "BI", "Addresses", "Stats" })]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddressStatDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<AddressStatDTO>>> GetAddressStat([FromQuery] string address, [FromQuery] long? epoch_no_min, [FromQuery] long? epoch_no_max, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (_context.AddressStat == null || _context.StakeAddress == null)
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1, (long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1, (long)page_no);

            long epochNoMin = epoch_no_min == null ? 0 : Math.Max(0, (long)epoch_no_min);
            long epochNoMax = epoch_no_max == null ? 1000000 : Math.Max(epochNoMin, (long)epoch_no_max);

            List<AddressStatDTO> stats = null;

            if (orderDir == "desc")
            {
                // Handle staking address
                if (address.Substring(0, 5).Equals("stake"))
                {
                    stats = await (
                            from ast in _context.AddressStat
                            join sa in _context.StakeAddress on ast.stake_address_id equals sa.id
                            where sa.view == address && ast.epoch_no >= epochNoMin && ast.epoch_no <= epochNoMax
                            orderby ast.epoch_no descending, ast.address descending
                            select new AddressStatDTO()
                            {
                                epoch_no = ast.epoch_no,
                                address = ast.address,
                                stake_address = sa.view,
                                tx_count = ast.tx_count
                            }).Skip((int)((pageNo - 1) * pageSize)).Take((int)pageSize).ToListAsync();
                }
                else
                {
                    // Handle enterprise and payment address
                    stats = await (
                        from ast in _context.AddressStat
                        join sa in _context.StakeAddress on ast.stake_address_id equals sa.id into saGroup
                        from sag in saGroup.DefaultIfEmpty()
                        where ast.address == address && ast.epoch_no >= epochNoMin && ast.epoch_no <= epochNoMax
                        orderby ast.epoch_no descending, ast.address descending
                        select new AddressStatDTO()
                        {
                            epoch_no = ast.epoch_no,
                            address = ast.address,
                            stake_address = sag.view != null ? sag.view : "",
                            tx_count = ast.tx_count
                        }).Skip((int)((pageNo - 1) * pageSize)).Take((int)pageSize).ToListAsync();
                }
            }
            else
            {
                // Handle staking address
                if (address.Substring(0, 5).Equals("stake"))
                {
                    stats = await (
                            from ast in _context.AddressStat
                            join sa in _context.StakeAddress on ast.stake_address_id equals sa.id
                            where sa.view == address && ast.epoch_no >= epochNoMin && ast.epoch_no <= epochNoMax
                            orderby ast.epoch_no ascending, ast.address ascending
                            select new AddressStatDTO()
                            {
                                epoch_no = ast.epoch_no,
                                address = ast.address,
                                stake_address = sa.view,
                                tx_count = ast.tx_count
                            }).Skip((int)((pageNo - 1) * pageSize)).Take((int)pageSize).ToListAsync();
                }
                else
                {
                    // Handle enterprise and payment address
                    stats = await (
                        from ast in _context.AddressStat
                        join sa in _context.StakeAddress on ast.stake_address_id equals sa.id into saGroup
                        from sag in saGroup.DefaultIfEmpty()
                        where ast.address == address && ast.epoch_no >= epochNoMin && ast.epoch_no <= epochNoMax
                        orderby ast.epoch_no ascending, ast.address ascending
                        select new AddressStatDTO()
                        {
                            epoch_no = ast.epoch_no,
                            address = ast.address,
                            stake_address = sag.view != null ? sag.view : "",
                            tx_count = ast.tx_count
                        }).Skip((int)((pageNo - 1) * pageSize)).Take((int)pageSize).ToListAsync();
                }
            }

            if (stats == null) return NotFound();

            // return stats;

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(stats);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }
        // public async Task<ActionResult<IEnumerable<AddressStat>>> GetAddressStatForStakeAddress(string address, [FromQuery] long? epoch_no_min, [FromQuery] long? epoch_no_max, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        // {
        //     if (_context.AddressStat == null)
        //     {
        //         return NotFound();
        //     }
        //     if (stake_address is null && epoch_no is null) return BadRequest("stake_address or epoch_no should not be null!");

        //     if (stake_address is not null && epoch_no is null)
        //         return await _context.AddressStat.Where(b => b.stake_address == stake_address).OrderBy(b => b.epoch_no).ToListAsync();
        //     else if (stake_address is null && epoch_no is not null)
        //         return await _context.AddressStat.Where(b => b.epoch_no == epoch_no).OrderBy(b => b.stake_address).ToListAsync();

        //     return await _context.AddressStat.Where(b => b.stake_address == stake_address).Where(b => b.epoch_no == epoch_no).ToListAsync();
        // }
    }
}
