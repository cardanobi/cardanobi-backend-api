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
    [Route("api/core/odata/blocks")]
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    public class BlocksController : ODataController
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<BlocksController> _logger;

        public BlocksController(cardanobiCoreContext context, ILogger<BlocksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Block history.</summary>
        /// <remarks>Returns the history of blocks starting from the latest block.</remarks>
        /// <param name="block_no">Block number to search from - defaults to the latest known block</param>
        /// <param name="depth">Number of blocks to return - defaults to 20 - max 100</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 100)]
        [HttpGet()]
        [SwaggerOperation(Tags = new []{"Core", "Blocks", "History"})]
        public async Task<ActionResult<IEnumerable<Block>>> GetBlock([FromQuery] long? block_no, [FromQuery] int? depth)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }

            long latestBlockNo = block_no == null ? (long)_context.Block.Max(b => b.block_no) : (long)block_no;
            int histDepth = depth == null ? 20 : Math.Min(100, (int)depth);

            var lastN = (
                 from b in _context.Block
                 where b.block_no <= latestBlockNo
                 orderby b.block_no descending
                 select new { b.id }).Take(histDepth).ToList().Select(x => x.id).ToArray();

            // _logger.LogInformation($"BlocksController.GetBlockPrevHistory: lastN {lastN}");

            return await _context.Block.Where(b => lastN.Contains(b.id)).ToListAsync();
        }
    }
}
