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
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    public class BlocksController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<BlocksController> _logger;

        public BlocksController(cardanobiCoreContext context, ILogger<BlocksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>Latest block.</summary>
        /// <remarks>Returns the latest block i.e. the tip of the blockchain.</remarks>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/blocks/latest")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<Block>> GetLatestBlock()
        {
            if (_context.Block == null)
            {
                return NotFound();
            }

            long latestBlockId = _context.Block.Max(b => b.id);
            _logger.LogInformation($"BlocksController.GetLatestBlock: latestBlockId {latestBlockId}");

            var block = await _context.Block.Where(b => b.id == latestBlockId).SingleOrDefaultAsync();

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        /// <summary>One block by block number.</summary>
        /// <remarks>Returns one specific block given its number.</remarks>
        /// <param name="block_no">Block number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/blocks/{block_no:long}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<Block>> GetBlock(long block_no)
        {
          if (_context.Block == null)
          {
              return NotFound();
          }
            var block = await _context.Block.Where(b => b.block_no == block_no).SingleOrDefaultAsync();

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        /// <summary>One block by block hash.</summary>
        /// <remarks>Returns one specific block given its hash.</remarks>
        /// <param name="block_hash">Block hash</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/blocks/{block_hash:length(64)}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<Block>> GetBlock(string block_hash)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }
            try {
                byte[] _res = Convert.FromHexString(block_hash);
            }
            catch(Exception e)
            {
                return NotFound();
            }
            var block = await _context.Block.Where(b => b.hash == Convert.FromHexString(block_hash)).SingleOrDefaultAsync();

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        /// <summary>One block by epoch and slot number.</summary>
        /// <remarks>Returns one specific block given its epoch and slot numbers.</remarks>
        /// <param name="epoch_no">Epoch number</param>
        /// <param name="slot_no">Slot number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/blocks/epochs/{epoch_no:long}/slots/{slot_no:long}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<Block>> GetBlock(long epoch_no, long slot_no)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }
            
            var block = await _context.Block
                .Where(b => b.epoch_no == epoch_no)
                .Where(b => b.slot_no == slot_no)
                .SingleOrDefaultAsync();

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        /// <summary>Block preceding history.</summary>
        /// <remarks>Returns the history of blocks preceding given block number.</remarks>
        /// <param name="block_no">Block Number</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/blocks/history/prev/{block_no}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<IEnumerable<Block>>> GetBlockHistory(long block_no)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }

            // _logger.LogInformation($"BlocksController.GetBlockHistory: block_no {block_no}");
            // return await _context.Block.Where(b => b.block_no < block_no).OrderByDescending(b => b.id).ToListAsync();

            var last5 = (
                 from b in _context.Block
                 where b.block_no < block_no
                 orderby b.block_no descending
                 select new { b.id}).Take(5).ToArray();
            long[] last5a = new long[5];
            int i=0;
            foreach (var bl in last5)
            {
                // last5a[i] = bl;
                i++;
            }

            // long[] last5a=new long[]{8403529,8403528,8403527,8403526,8403525};

            _logger.LogInformation($"BlocksController.GetBlockHistory: last5 {last5}");

            return await _context.Block.Where(b => last5a.Contains(b.id)).OrderByDescending(b => b.id).ToListAsync();

            // var block = await (
            //     from b in _context.Block
            //     where b.block_no < block_no
            //     orderby b.block_no descending
            //     select b).ToListAsync();

            // if (block == null) return NotFound();
            // return block;
        }

        /// <summary>Latest block for a given pool.</summary>
        /// <remarks>Returns the latest block forged by a pool given its pool identifier.</remarks>
        /// <param name="pool_hash">The Bech32 or HEX encoding of the pool hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/blocks/latest/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<Block>> GetLatestBlock(string pool_hash)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }

            var isBech32 = pool_hash.Substring(0,4).Equals("pool") ? true : false;
                    
            if(isBech32) {
                var query = (
                        from b1 in _context.Block
                        where b1.id == (
                            from b in _context.Block
                            join sl in _context.SlotLeader on b.slot_leader_id equals sl.id  
                            join ph in _context.PoolHash on sl.pool_hash_id equals ph.id
                            where ph.view == pool_hash
                            select b.id
                        ).Max()
                        select b1);
                var block = await query.SingleOrDefaultAsync().ConfigureAwait(false);

                if (block == null) return NotFound();
                return block;
            } else {
                var poolHashRaw = Convert.FromHexString(pool_hash);
                var query = (
                    from b1 in _context.Block
                    where b1.id == (
                        from b in _context.Block
                        join sl in _context.SlotLeader on b.slot_leader_id equals sl.id  
                        join ph in _context.PoolHash on sl.pool_hash_id equals ph.id
                        where ph.hash_raw == poolHashRaw
                        select b.id
                    ).Max()
                    select b1);
                var block = await query.SingleOrDefaultAsync().ConfigureAwait(false);

                if (block == null) return NotFound();
                return block;
            }
        }

        /// <summary>Block history for a given pool.</summary>
        /// <remarks>Returns the history of blocks forged by a pool given its pool identifier.</remarks>
        /// <param name="pool_hash">The Bech32 or HEX encoding of the pool hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/blocks/history/pools/{pool_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Blocks"})]
        public async Task<ActionResult<IEnumerable<Block>>> GetBlockHistory(string pool_hash)
        {
            if (_context.Block == null)
            {
                return NotFound();
            }

            var isBech32 = pool_hash.Substring(0,4).Equals("pool") ? true : false;
                    
            if(isBech32) {
                var block = await (
                        from b in _context.Block
                        join sl in _context.SlotLeader on b.slot_leader_id equals sl.id  
                        join ph in _context.PoolHash on sl.pool_hash_id equals ph.id
                        where ph.view == pool_hash
                        select b).ToListAsync();

                if (block == null) return NotFound();
                return block;
            } else {
                var poolHashRaw = Convert.FromHexString(pool_hash);
                var block = await (
                        from b in _context.Block
                        join sl in _context.SlotLeader on b.slot_leader_id equals sl.id  
                        join ph in _context.PoolHash on sl.pool_hash_id equals ph.id
                        where ph.hash_raw == poolHashRaw
                        select b).ToListAsync();

                if (block == null) return NotFound();
                return block;
            }
        }
    }
}
