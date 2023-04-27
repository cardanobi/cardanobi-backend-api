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
using System.Text;

namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    public class AssetsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<AssetsController> _logger;

        public AssetsController(cardanobiCoreContext context, ILogger<AssetsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>All assets.</summary>
        /// <remarks>Returns the list of multi assets minted on Cardano.</remarks>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order assets are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/assets")]
        [SwaggerOperation(Tags = new []{"Core", "Assets"})]
        public async Task<ActionResult<IEnumerable<AssetListDTO>>> GetAssets([FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.MultiAsset == null ||
                _context.MultiAssetCache == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long latestMultiAssetId = _context.MultiAssetCache.Max(b => b.asset_id);;
            long pageSize = page_size == null ? 20 : Math.Min(100, (long)page_size);
            long maxPageNo = (latestMultiAssetId % pageSize == 0) ? latestMultiAssetId / pageSize : latestMultiAssetId / pageSize + 1;
            long pageNo = page_no == null ? 1 : Math.Min(maxPageNo, Math.Max(1,(long)page_no));
  
            _logger.LogInformation($"AssetsController.GetAssets2: orderDir {orderDir}, pageNo {pageNo}, latestMultiAssetId {latestMultiAssetId}, pageSize {pageSize}, maxPageNo {maxPageNo}");

            IEnumerable<AssetListDTO> assets = null;

            if (orderDir == "desc") 
            {
                assets = await (
                    from ma in _context.MultiAsset
                    join mac in _context.MultiAssetCache on ma.id equals mac.asset_id
                    orderby ma.id descending
                    select new AssetListDTO()
                    {
                        asset_id = ma.id,
                        fingerprint = ma.fingerprint,
                        policy_hex = ma.policy_hex,
                        total_supply = mac.total_supply
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                assets = await (
                    from ma in _context.MultiAsset
                    join mac in _context.MultiAssetCache on ma.id equals mac.asset_id
                    orderby ma.id ascending
                    select new AssetListDTO()
                    {
                        asset_id = ma.id,
                        fingerprint = ma.fingerprint,
                        policy_hex = ma.policy_hex,
                        total_supply = mac.total_supply
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }
  
            if (assets == null)
            {
                return NotFound();
            }
            
            return Ok(assets);
        }

        /// <summary>One asset.</summary>
        /// <remarks>Returns the details of one multi asset minted on Cardano given its fingerprint.</remarks>
        /// <param name="fingerprint">The CIP14 fingerprint for the MultiAsset.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/assets/{fingerprint}")]
        [SwaggerOperation(Tags = new []{"Core", "Assets"})]
        public async Task<ActionResult<AssetDetailsDTO>> GetAssetByFingerprint(string fingerprint)
        {
            if (
                _context.MultiAsset == null ||
                _context.MultiAssetCache == null ||
                _context.TransactionMetadata == null
                )
            {
                return NotFound();
            }

            var asset = await (
                from ma in _context.MultiAsset
                join mac in _context.MultiAssetCache on ma.id equals mac.asset_id
                join tm in _context.TransactionMetadata on mac.first_mint_tx_id equals tm.tx_id into tmGroup
                from tmg in tmGroup.DefaultIfEmpty()
                where ma.fingerprint == fingerprint
                select new AssetDetailsDTO()
                {
                    asset_id = ma.id,
                    fingerprint = ma.fingerprint,
                    policy_hex = ma.policy_hex,
                    name = ma.name != null ? Encoding.Default.GetString(ma.name) : null,
                    creation_time =  mac.creation_time,
                    total_supply = mac.total_supply,
                    mint_cnt = mac.mint_cnt,
                    burn_cnt = mac.burn_cnt,
                    first_mint_tx_hash = mac.first_mint_tx_hash,
                    first_mint_keys = mac.first_mint_keys,
                    last_mint_tx_hash = mac.last_mint_tx_hash,
                    last_mint_keys = mac.last_mint_keys,
                    first_mint_metadata = tmg.json
                }).ToListAsync();
  
            if (asset == null)
            {
                return NotFound();
            }
            
            return Ok(asset);
        }

        /// <summary>Asset history.</summary>
        /// <remarks>Returns the minting/burning history of one MultiAsset given its fingerprint.</remarks>
        /// <param name="fingerprint">The CIP14 fingerprint for the MultiAsset.</param>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order the minting/burning events are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/assets/{fingerprint}/history")]
        [SwaggerOperation(Tags = new []{"Core", "Assets", "History"})]
        public async Task<ActionResult<IEnumerable<AssetHistoryDTO>>> GetAssetHistory(string fingerprint, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.MultiAsset == null ||
                _context.MultiAssetCache == null
                )
            {
                return NotFound();
            }

            // var mintEventsL = await (
            //     from matm in _context.MultiAssetTransactionMint
            //     join ma in _context.MultiAsset on matm.ident equals ma.id
            //     where ma.fingerprint == fingerprint
            //     orderby matm.id descending
            //     select new {matm.id}).ToListAsync();
                
            // var mintEvents = mintEventsL.Select(i => i.id).ToArray();

            string orderDir = order == null ? "desc" : order;
            // long mintEventCount = mintEvents.Length;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            // long maxPageNo = (mintEventCount % pageSize == 0) ? mintEventCount / pageSize : mintEventCount / pageSize + 1;
            // long pageNo = page_no == null ? 1 : Math.Min(maxPageNo, Math.Max(1,(long)page_no));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            // _logger.LogInformation(@$"AssetsController.GetAssetHistory: orderDir {orderDir}, 
            //         mintEventCount {mintEventCount}, 
            //         pageSize {pageSize}, 
            //         maxPageNo {maxPageNo},
            //         pageNo {pageNo}");

            IEnumerable<AssetHistoryDTO> history = null;

            if (orderDir == "desc") 
            {
                history = await (
                    from matm in _context.MultiAssetTransactionMint
                    join ma in _context.MultiAsset on matm.ident equals ma.id
                    join tx in _context.Transaction on matm.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where ma.fingerprint == fingerprint
                    orderby matm.id descending
                    select new AssetHistoryDTO()
                    {
                        event_id = matm.id,
                        tx_hash_hex = tx.hash_hex,
                        quantity = matm.quantity,
                        event_time = b.time,
                        block_no = b.block_no
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                history = await (
                    from matm in _context.MultiAssetTransactionMint
                    join ma in _context.MultiAsset on matm.ident equals ma.id
                    join tx in _context.Transaction on matm.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where ma.fingerprint == fingerprint
                    select new AssetHistoryDTO()
                    {
                        event_id = matm.id,
                        tx_hash_hex = tx.hash_hex,
                        quantity = matm.quantity,
                        event_time = b.time,
                        block_no = b.block_no
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (history == null)
            {
                return NotFound();
            }
            
            return Ok(history);
        }

        /// <summary>Asset transactions.</summary>
        /// <remarks>Returns details of transactions involving one MultiAsset given its fingerprint.</remarks>
        /// <param name="fingerprint">The CIP14 fingerprint for the MultiAsset.</param>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order transactions are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/assets/{fingerprint}/transactions")]
        [SwaggerOperation(Tags = new []{"Core", "Assets", "Transactions"})]
        public async Task<ActionResult<IEnumerable<AssetTransactionDTO>>> GetAssetTransactions(string fingerprint, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.MultiAssetTransactionOutput == null ||
                _context.MultiAsset == null ||
                _context.TransactionOutput == null ||
                _context.Transaction == null ||
                _context.Block == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AssetTransactionDTO> transactions = null;

            if (orderDir == "desc") 
            {
                transactions = await (
                    from mato in _context.MultiAssetTransactionOutput
                    join ma in _context.MultiAsset on mato.ident equals ma.id
                    join txo in _context.TransactionOutput on mato.tx_out_id equals txo.id
                    join tx in _context.Transaction on txo.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where ma.fingerprint == fingerprint
                    orderby mato.id descending
                    select new AssetTransactionDTO()
                    {
                        tx_id = tx.id,
                        hash_hex = tx.hash_hex,
                        epoch_no = b.epoch_no,
                        block_no = b.block_no,
                        event_time = b.time
                    }).Distinct().OrderByDescending(x => x.tx_id).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                transactions = await (
                    from mato in _context.MultiAssetTransactionOutput
                    join ma in _context.MultiAsset on mato.ident equals ma.id
                    join txo in _context.TransactionOutput on mato.tx_out_id equals txo.id
                    join tx in _context.Transaction on txo.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where ma.fingerprint == fingerprint
                    orderby mato.id ascending
                    select new AssetTransactionDTO()
                    {
                        tx_id = tx.id,
                        hash_hex = tx.hash_hex,
                        epoch_no = b.epoch_no,
                        block_no = b.block_no,
                        event_time = b.time
                    }).Distinct().OrderBy(x => x.tx_id).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (transactions == null)
            {
                return NotFound();
            }
            
            return Ok(transactions);
        }

        /// <summary>Asset addresses.</summary>
        /// <remarks>Returns the list of addresses holding a balance in one specific MultiAsset given its fingerprint.</remarks>
        /// <param name="fingerprint">The CIP14 fingerprint for the MultiAsset.</param>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order addresses are returned - "desc" descending (default) quantity held - "asc" ascending quantity held</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/assets/{fingerprint}/addresses")]
        [SwaggerOperation(Tags = new []{"Core", "Assets", "Addresses"})]
        public async Task<ActionResult<IEnumerable<AssetAddressDTO>>> GetAssetAddresses(string fingerprint, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.MultiAssetAddressCache == null ||
                _context.MultiAsset == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AssetAddressDTO> transactions = null;

            if (orderDir == "desc") 
            {
                transactions = await (
                    from maac in _context.MultiAssetAddressCache
                    join ma in _context.MultiAsset on maac.asset_id equals ma.id
                    where ma.fingerprint == fingerprint
                    orderby maac.quantity descending
                    select new AssetAddressDTO()
                    {
                        address = maac.address,
                        quantity = maac.quantity
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                transactions = await (
                    from maac in _context.MultiAssetAddressCache
                    join ma in _context.MultiAsset on maac.asset_id equals ma.id
                    where ma.fingerprint == fingerprint
                    orderby maac.quantity ascending
                    select new AssetAddressDTO()
                    {
                        address = maac.address,
                        quantity = maac.quantity
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (transactions == null)
            {
                return NotFound();
            }
            
            return Ok(transactions);
        }

        /// <summary>Asset list by policy.</summary>
        /// <remarks>Returns the list of MultiAsset for a specific policy given its hash.</remarks>
        /// <param name="policy_hash">The MultiAsset policy hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/assets/policies/{policy_hash}")]
        [SwaggerOperation(Tags = new []{"Core", "Assets"})]
        public async Task<ActionResult<IEnumerable<AssetPolicyDTO>>> GetAssetByPolicy(string policy_hash)
        {
            if (
                _context.MultiAssetAddressCache == null ||
                _context.MultiAsset == null
                )
            {
                return NotFound();
            }
            try {
                byte[] _res = Convert.FromHexString(policy_hash);
            }
            catch(Exception e)
            {
                return NotFound();
            }

            var assets = await (
                    from mac in _context.MultiAssetCache
                    join ma in _context.MultiAsset on mac.asset_id equals ma.id
                    where ma.policy == Convert.FromHexString(policy_hash)
                    orderby ma.id
                    select new AssetPolicyDTO()
                    {
                        fingerprint = ma.fingerprint,
                        total_supply = mac.total_supply
                    }).ToListAsync();
            
            if (assets == null)
            {
                return NotFound();
            }
            
            return Ok(assets);
        }
    }
}
