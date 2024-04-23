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

namespace ApiCore.Controllers.Odata
{
    [Route("api/core/odata/epochsstakes")]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    // [ApiExplorerSettings(GroupName = "OData/EpochsStakes")]
    public class EpochsStakesViewsController : ODataController
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<EpochsStakesViewsController> _logger;

        public EpochsStakesViewsController(cardanobiCoreContext context, ILogger<EpochsStakesViewsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>One epoch and one pool stake distributions.</summary>
        /// <remarks>Returns the stake distribution for one epoch given its number, and for one pool given its Bech32 pool hash.</remarks>
        /// <param name="epoch_no">Epoch number</param>
        /// <param name="pool_hash">Bech32 pool hash</param>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order stakes are returned - "desc" descending (default) from largest to smallest stake amount - "asc" ascending from smallest to largest stake amount</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        [EnableQuery(PageSize = 100)]
        [HttpGet]
        [SwaggerOperation(Tags = new []{"Core", "Epochs", "Stakes" })]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ActivePoolStakePerPoolPerEpochDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ActivePoolStakePerPoolPerEpochDTO>>> GetActivePoolStakePerPoolPerEpoch([FromQuery] long? epoch_no, [FromQuery] string? pool_hash, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.ActiveStakeCacheAccount == null ||
                _context.PoolHash == null || 
                _context.StakeAddress == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            var recordsCount = (
                from casca in _context.ActiveStakeCacheAccount  // Active stake cache account records
                join ph in _context.PoolHash on casca.pool_hash_id equals ph.id              // Join condition
                where ph.view == pool_hash && casca.epoch_no == epoch_no
                select casca                                  // Select any field from the joined records
            ).Count();  // Count the number of matched records

            long pageSize = page_size == null ? 20 : Math.Min(100, (long)page_size);
            long maxPageNo = (recordsCount % pageSize == 0) ? recordsCount / pageSize : recordsCount / pageSize + 1;
            long pageNo = page_no == null ? 1 : Math.Min(maxPageNo, Math.Max(1,(long)page_no));
  
            _logger.LogInformation($"EpochsStakesViewsController.GetActivePoolStakePerEpoch: pageNo {pageNo}, recordsCount {recordsCount}, pageSize {pageSize}, maxPageNo {maxPageNo}");

            IEnumerable<ActivePoolStakePerPoolPerEpochDTO> stakes = null;

            if (orderDir == "desc") 
            {
                stakes = await (
                    from casca in _context.ActiveStakeCacheAccount  // Active stake cache account records
                    join ph in _context.PoolHash on casca.pool_hash_id equals ph.id              // Join condition
                    join sa in _context.StakeAddress on casca.stake_address_id equals sa.id
                    where ph.view == pool_hash && casca.epoch_no == epoch_no
                    orderby casca.amount descending
                    select new ActivePoolStakePerPoolPerEpochDTO()
                    {
                        stake_address = sa.view,
                        amount = casca.amount
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                stakes = await (
                    from casca in _context.ActiveStakeCacheAccount  // Active stake cache account records
                    join ph in _context.PoolHash on casca.pool_hash_id equals ph.id              // Join condition
                    join sa in _context.StakeAddress on casca.stake_address_id equals sa.id
                    where ph.view == pool_hash && casca.epoch_no == epoch_no
                    orderby casca.amount ascending
                    select new ActivePoolStakePerPoolPerEpochDTO()
                    {
                        stake_address = sa.view,
                        amount = casca.amount
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();   
            }
            
  
            if (stakes == null)
            {
                return NotFound();
            }
            
            // return Ok(stakes);

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(stakes);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");   
        }
    }
}
