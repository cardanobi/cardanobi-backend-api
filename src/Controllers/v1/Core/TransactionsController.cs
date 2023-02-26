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

namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;

        public TransactionsController(cardanobiCoreContext context)
        {
            _context = context;
        }

        /// <summary>Details of a given transaction.</summary>
        /// <remarks>Returns of a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}")]
        [SwaggerOperation(Tags = new[] { "Core", "Blocks", "Pools" })]
        public async Task<ActionResult<TransactionDTO>> GetTransaction(string transaction_hash)
        {
            if (
                _context.Transaction == null ||
                _context.Block == null ||
                _context.TransactionOutput == null ||
                _context.MultiAssetTransactionOutput == null ||
                _context.MultiAsset == null)
            {
                return NotFound();
            }
            try {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch(Exception e)
            {
                return NotFound();
            }

            // IEnumerable<TransactionDTO> transaction =
            //     from tx in _context.Transaction
            //     join b in _context.Block on tx.block_id equals b.id
            //     where tx.hash = Convert.FromHexString(transaction_hash)
            //     select new TransactionDTO()
            //     {
            //         id = tx.id,
            //         hash_hex = tx.hash_hex,
            //         block_no = b.block_no,
            //         slot_no = b.slot_no,
            //         block_time = b.time,
            //         block_index = tx.block_index,
            //         out_sum = tx.out_sum,
            //         fee = tx.fee,
            //         deposit = tx.deposit,
            //         size = tx.size,
            //         script_size = tx.script_size,
            //         invalid_before = tx.invalid_before,
            //         invalid_hereafter = tx.invalid_hereafter,
            //         valid_contract = tx.valid_contract,
            //         outputs = (
            //             from to2 in _context.TransactionOutput
            //             join mto in _context.MultiAssetTransactionOutput on to2.id equals mto.tx_out_id
            //             join ma in _context.MultiAsset on mto.ident equals ma.id
            //             where to2.tx_id = tx.id
            //             select to2.index, mto.quantity, ma.name, ma.fingerprint
            //         ).ToList()
            //     };

            IEnumerable<TransactionDTO> transaction =
                from tx in _context.Transaction
                join b in _context.Block on tx.block_id equals b.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionDTO()
                {
                    id = tx.id,
                    hash_hex = tx.hash_hex,
                    block_no = b.block_no,
                    slot_no = b.slot_no,
                    block_time = b.time,
                    block_index = tx.block_index,
                    out_sum = tx.out_sum,
                    fee = tx.fee,
                    deposit = tx.deposit,
                    size = tx.size,
                    script_size = tx.script_size,
                    invalid_before = tx.invalid_before,
                    invalid_hereafter = tx.invalid_hereafter,
                    valid_contract = tx.valid_contract,
                };

            // IEnumerable<TransactionOutputDTO> transaction_outputs =
            //     from to2 in _context.TransactionOutput
            //     join mto in _context.MultiAssetTransactionOutput on to2.id equals mto.tx_out_id
            //     join ma in _context.MultiAsset on mto.ident equals ma.id
            //     where to2.tx_id = tx.id
            //     select new TransactionDTO()
            //     {
            //         index = to2.index,
            //         quantity = mto.quantity,
            //         name = ma.name,
            //         fingerprint = ma.fingerprint
            //     }.ToList();

                if (transaction == null) return NotFound();
                return Ok(transaction);
        }

    }
}
