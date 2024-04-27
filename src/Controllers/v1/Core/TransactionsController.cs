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
using Npgsql;

namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy="core-read")]
    [Produces("application/json")]
    public class TransactionsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly cardanobiCoreContext2 _context2;
        private readonly cardanobiCoreContext3 _context3;

        public TransactionsController(cardanobiCoreContext context, cardanobiCoreContext2 context2, cardanobiCoreContext3 context3)
        {
            _context = context;
            _context2 = context2;
            _context3 = context3;
        }

        /// <summary>Details of a given transaction.</summary>
        /// <remarks>Returns details of a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions" })]
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
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            // IEnumerable<TransactionDTO> transaction =
            var transaction = await (
                from tx in _context.Transaction
                join b in _context.Block on tx.block_id equals b.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionDTO()
                {
                    id = tx.id,
                    tx_hash_hex = tx.hash_hex,
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
                }).SingleOrDefaultAsync();

            if (transaction == null) return NotFound();

            Task<List<TransactionAmountDTO>> t_amounts = Task<List<TransactionAmountDTO>>.Run(() =>
            {
                var output_amounts = (
                    from txo in _context.TransactionOutput
                    join mto in _context.MultiAssetTransactionOutput on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context.MultiAsset on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where txo.tx_id == transaction.id
                    group new { txo, mag, mtog } by new { txo.index, mag.fingerprint, mag.name } into g
                    orderby g.Key.index
                    select new TransactionOutputProjectionDTO()
                    {
                        output_index = g.Key.index,
                        asset_name = g.Key.name != null ? Encoding.Default.GetString(g.Key.name) : "",
                        asset_fingerprint = g.Key.fingerprint,
                        lovelace_value = (ulong)g.Sum(b => (decimal)b.txo.value),
                        asset_quantity = (ulong)g.Sum(b => (decimal)b.mtog.quantity)
                    }).ToList();

                // Preparing the amounts summary (total lovelace value + total multi asset values )
                List<TransactionAmountDTO> amounts = new List<TransactionAmountDTO>();
                ulong lovelaceValue = 0;
                long lastGroup = -1;
                int outputCount = 0;

                foreach (TransactionOutputProjectionDTO po in output_amounts)
                {
                    if (po.output_index > lastGroup)
                    {
                        // Adding all lovelace output values
                        lovelaceValue = lovelaceValue + po.lovelace_value;
                        lastGroup = po.output_index;
                        outputCount++;
                    }

                    if (po.asset_fingerprint != null)
                    {
                        // Referencing all multi assets envolved in outputs
                        TransactionAmountDTO asset_amount = new TransactionAmountDTO();
                        asset_amount.value = po.asset_quantity;
                        asset_amount.unit = po.asset_name;
                        asset_amount.asset_fingerprint = po.asset_fingerprint;

                        amounts.Add(asset_amount);
                    }
                }

                amounts = amounts.OrderBy(x => x.unit).ToList();

                TransactionAmountDTO lovelace_amount = new TransactionAmountDTO();
                lovelace_amount.value = lovelaceValue;
                lovelace_amount.unit = "lovelace";
                amounts.Insert(0, lovelace_amount);

                transaction.outputCount = outputCount;

                return amounts;
            });

            // Ref tables count
            Task<bool> t_count_part1 = Task<bool>.Run(() =>
            {
                transaction.inputCount = (
                    from txo in _context2.TransactionOutput
                    join tx_in in _context2.TransactionInput on txo.tx_id equals tx_in.tx_out_id
                    join tx in _context2.Transaction on tx_in.tx_in_id equals tx.id
                    where txo.index == tx_in.tx_out_index && tx.id == transaction.id
                    select txo.id).Count();

                transaction.withdrawalCount = _context2.Withdrawal.Count(p => p.tx_id == transaction.id);
                transaction.assetMintCount = _context2.MultiAssetTransactionMint.Count(p => p.tx_id == transaction.id);
                transaction.metadataCount = _context2.TransactionMetadata.Count(p => p.tx_id == transaction.id);
                transaction.stakeRegistrationCount = _context2.StakeRegistration.Count(p => p.tx_id == transaction.id);
                transaction.stakeDeregistrationCount = _context2.StakeDeregistration.Count(p => p.tx_id == transaction.id);
                transaction.redeemerCount = _context2.Redeemer.Count(p => p.tx_id == transaction.id);

                return true;
            });

            Task<bool> t_count_part2 = Task<bool>.Run(() =>
            {
                transaction.delegationCount = _context3.Delegation.Count(p => p.tx_id == transaction.id);
                transaction.treasuryCount = _context3.Treasury.Count(p => p.tx_id == transaction.id);
                transaction.reserveCount = _context3.Reserve.Count(p => p.tx_id == transaction.id);
                transaction.potTransferCount = _context3.PotTransfer.Count(p => p.tx_id == transaction.id);
                transaction.paramProposalCount = _context3.ParamProposal.Count(p => p.registered_tx_id == transaction.id);
                transaction.poolRetireCount = _context3.PoolRetire.Count(p => p.announced_tx_id == transaction.id);
                transaction.poolUpdateCount = _context3.PoolUpdate.Count(p => p.registered_tx_id == transaction.id);

                return true;
            });

            Task.WaitAll(t_amounts, t_count_part1, t_count_part2);

            // get results from these tasks
            transaction.output_amounts = t_amounts.Result;

            return Ok(transaction);
        }

        /// <summary>Inputs and Unspent Outputs of a given transaction.</summary>
        /// <remarks>Returns all Inputs and Unspent Outputs (UTXOs) of a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/utxos")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions" })]
        public async Task<ActionResult<TransactionUtxoDTO>> GetTransactionUTXO(string transaction_hash)
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
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var transaction = await (
                from tx in _context.Transaction.AsNoTracking()
                join b in _context.Block.AsNoTracking() on tx.block_id equals b.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionUtxoDTO()
                {
                    id = tx.id,
                    tx_hash_hex = tx.hash_hex,
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
                }).SingleOrDefaultAsync();

            if (transaction == null) return NotFound();

            // OUTPUTS
            Task<List<TransactionOutputProjectionDTO>> t_main_outputs = Task<List<TransactionOutputProjectionDTO>>.Run(() =>
            {
                var main_outputs = (
                    from txo in _context.TransactionOutput.AsNoTracking()
                    join da in _context.Datum.AsNoTracking() on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context.Script.AsNoTracking() on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context.MultiAssetTransactionOutput.AsNoTracking() on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context.MultiAsset.AsNoTracking() on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where txo.tx_id == transaction.id
                    select new TransactionOutputProjectionDTO()
                    {
                        output_index = txo.index,
                        lovelace_value = txo.value,
                        address = txo.address,
                        hash_hex = transaction.tx_hash_hex,
                        is_collateral = false,
                        data_hash = txo.data_hash != null ? Convert.ToHexString(txo.data_hash).ToLower() : null,
                        inline_datum_cbor = dag.bytes != null ? Convert.ToHexString(dag.bytes).ToLower() : null,
                        reference_script_hash = scg.hash != null ? Convert.ToHexString(scg.hash).ToLower() : null,
                        asset_quantity = mtog != null ? mtog.quantity : 0,
                        asset_name = mag != null ? Encoding.Default.GetString(mag.name) : null,
                        asset_fingerprint = mag != null ? mag.fingerprint : null
                    }).ToList();

                return main_outputs;
            });

            Task<List<TransactionOutputProjectionDTO>> t_collateral_outputs = Task<List<TransactionOutputProjectionDTO>>.Run(() =>
            {
                var collateral_outputs = (
                    from cto in _context2.CollateralTransactionOutput.AsNoTracking()
                    join da in _context2.Datum.AsNoTracking() on cto.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context2.Script.AsNoTracking() on cto.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context2.MultiAssetTransactionOutput.AsNoTracking() on cto.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context2.MultiAsset.AsNoTracking() on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where cto.tx_id == transaction.id
                    select new TransactionOutputProjectionDTO()
                    {
                        output_index = cto.index,
                        lovelace_value = cto.value,
                        address = cto.address,
                        hash_hex = transaction.tx_hash_hex,
                        is_collateral = true,
                        data_hash = cto.data_hash != null ? Convert.ToHexString(cto.data_hash).ToLower() : null,
                        inline_datum_cbor = dag.bytes != null ? Convert.ToHexString(dag.bytes).ToLower() : null,
                        reference_script_hash = scg.hash != null ? Convert.ToHexString(scg.hash).ToLower() : null,
                        asset_quantity = mtog != null ? mtog.quantity : 0,
                        asset_name = mag != null ? Encoding.Default.GetString(mag.name) : null,
                        asset_fingerprint = mag != null ? mag.fingerprint : null
                    }).ToList();

                    
                return collateral_outputs;
            });

            Task.WaitAll(t_main_outputs, t_collateral_outputs);

            // get results from these
            var main_outputs = t_main_outputs.Result;
            var collateral_outputs = t_collateral_outputs.Result;

            // Preparing the outputs
            List<TransactionOutputDTO> outputs = new List<TransactionOutputDTO>();

            foreach (TransactionOutputProjectionDTO po in main_outputs.Concat(collateral_outputs)) 
            {
                var existingOutput = outputs.FirstOrDefault(u => u.output_index.Equals(po.output_index));
                if (existingOutput == null) 
                {
                    TransactionOutputDTO newOutput = new TransactionOutputDTO();
                    newOutput.output_index = po.output_index;
                    newOutput.address = po.address;
                    newOutput.tx_hash_hex = po.hash_hex;
                    newOutput.is_collateral = po.is_collateral;
                    newOutput.data_hash = po.data_hash;
                    newOutput.inline_datum_cbor = po.inline_datum_cbor;
                    newOutput.reference_script_hash = po.reference_script_hash;
                    newOutput.amounts = new List<TransactionAmountDTO>();

                    TransactionAmountDTO lovelace = new TransactionAmountDTO();
                    lovelace.value = po.lovelace_value;
                    lovelace.unit = "lovelace";

                    newOutput.amounts.Add(lovelace);

                    if (po.asset_fingerprint != null) 
                    {
                        TransactionAmountDTO asset = new TransactionAmountDTO();
                        asset.value = po.asset_quantity;
                        asset.unit = po.asset_name;
                        asset.asset_fingerprint = po.asset_fingerprint;

                        newOutput.amounts.Add(asset);
                    }

                    outputs.Add(newOutput);
                } else {
                    if (po.asset_fingerprint != null) 
                    {
                        TransactionAmountDTO asset = new TransactionAmountDTO();
                        asset.value = po.asset_quantity;
                        asset.unit = po.asset_name;
                        asset.asset_fingerprint = po.asset_fingerprint;

                        existingOutput.amounts.Add(asset);
                    }
                }
            }

            transaction.outputs = outputs;

            // INPUTS
            Task<List<TransactionInputProjectionDTO>> t_main_inputs = Task<List<TransactionInputProjectionDTO>>.Run(() =>
            {
                var main_inputs = (
                    from txo in _context.TransactionOutput
                    join tx_in in _context.TransactionInput on txo.tx_id equals tx_in.tx_out_id
                    join tx in _context.Transaction on tx_in.tx_in_id equals tx.id
                    join tx2 in _context.Transaction on txo.tx_id equals tx2.id
                    join da in _context.Datum on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context.Script on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context.MultiAssetTransactionOutput on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context.MultiAsset on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where txo.index == tx_in.tx_out_index && tx.hash == Convert.FromHexString(transaction_hash)
                    select new TransactionInputProjectionDTO()
                    {
                        output_index = txo.index,
                        lovelace_value = txo.value,
                        address = txo.address,
                        hash_hex = Convert.ToHexString(tx2.hash).ToLower(),
                        is_collateral = false,
                        is_reference = false,
                        data_hash = txo.data_hash != null ? Convert.ToHexString(txo.data_hash).ToLower() : null,
                        inline_datum_cbor = dag.bytes != null ? Convert.ToHexString(dag.bytes).ToLower() : null,
                        reference_script_hash = scg.hash != null ? Convert.ToHexString(scg.hash).ToLower() : null,
                        asset_quantity = mtog != null ? mtog.quantity : 0,
                        asset_name = mag != null ? Encoding.Default.GetString(mag.name) : null,
                        asset_fingerprint = mag != null ? mag.fingerprint : null
                    }).ToList();

                return main_inputs;
            });

            Task<List<TransactionInputProjectionDTO>> t_collateral_inputs = Task<List<TransactionInputProjectionDTO>>.Run(() =>
            {
                var collateral_inputs = (
                    from txo in _context2.TransactionOutput.AsNoTracking()
                    join ctx_in in _context2.CollateralTransactionInput.AsNoTracking() on txo.tx_id equals ctx_in.tx_out_id
                    join tx in _context2.Transaction.AsNoTracking() on txo.tx_id equals tx.id
                    join da in _context2.Datum.AsNoTracking() on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context2.Script.AsNoTracking() on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context2.MultiAssetTransactionOutput.AsNoTracking() on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context2.MultiAsset.AsNoTracking() on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where txo.index == ctx_in.tx_out_index && ctx_in.tx_in_id == transaction.id
                    select new TransactionInputProjectionDTO()
                    {
                        output_index = txo.index,
                        lovelace_value = txo.value,
                        address = txo.address,
                        hash_hex = tx.hash_hex,
                        is_collateral = true,
                        is_reference = false,
                        data_hash = txo.data_hash != null ? Convert.ToHexString(txo.data_hash).ToLower() : null,
                        inline_datum_cbor = dag.bytes != null ? Convert.ToHexString(dag.bytes).ToLower() : null,
                        reference_script_hash = scg.hash != null ? Convert.ToHexString(scg.hash).ToLower() : null,
                        asset_quantity = mtog != null ? mtog.quantity : 0,
                        asset_name = mag != null ? Encoding.Default.GetString(mag.name) : null,
                        asset_fingerprint = mag != null ? mag.fingerprint : null
                    }).ToList();

                return collateral_inputs;
            });

            Task<List<TransactionInputProjectionDTO>> t_reference_inputs = Task<List<TransactionInputProjectionDTO>>.Run(() =>
            {
                var reference_inputs = (
                    from txo in _context3.TransactionOutput.AsNoTracking()
                    join rtx_in in _context3.ReferenceTransactionInput.AsNoTracking() on txo.tx_id equals rtx_in.tx_out_id
                    join tx in _context3.Transaction.AsNoTracking() on txo.tx_id equals tx.id
                    join da in _context3.Datum.AsNoTracking() on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context3.Script.AsNoTracking() on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context3.MultiAssetTransactionOutput.AsNoTracking() on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context3.MultiAsset.AsNoTracking() on mtog.ident equals ma.id into maGroup
                    from mag in maGroup.DefaultIfEmpty()
                    where txo.index == rtx_in.tx_out_index && rtx_in.tx_in_id == transaction.id
                    select new TransactionInputProjectionDTO()
                    {
                        output_index = txo.index,
                        lovelace_value = txo.value,
                        address = txo.address,
                        hash_hex = tx.hash_hex,
                        is_collateral = false,
                        is_reference = true,
                        data_hash = txo.data_hash != null ? Convert.ToHexString(txo.data_hash).ToLower() : null,
                        inline_datum_cbor = dag.bytes != null ? Convert.ToHexString(dag.bytes).ToLower() : null,
                        reference_script_hash = scg.hash != null ? Convert.ToHexString(scg.hash).ToLower() : null,
                        asset_quantity = mtog != null ? mtog.quantity : 0,
                        asset_name = mag != null ? Encoding.Default.GetString(mag.name) : null,
                        asset_fingerprint = mag != null ? mag.fingerprint : null
                    }).ToList();

                return reference_inputs;
            });

            Task.WaitAll(t_main_inputs, t_collateral_inputs, t_reference_inputs);
            // Task.WaitAll(t_collateral_inputs, t_reference_inputs);

            // get results from these
            var main_inputs = t_main_inputs.Result;
            var collateral_inputs = t_collateral_inputs.Result;
            var reference_inputs = t_reference_inputs.Result;

            // Preparing the inputs
            List<TransactionInputDTO> inputs = new List<TransactionInputDTO>();

            foreach (TransactionInputProjectionDTO pi in main_inputs.Concat(collateral_inputs).Concat(reference_inputs)) 
            // foreach (TransactionInputProjectionDTO pi in collateral_inputs.Concat(reference_inputs)) 
            {
                var existingInput = inputs.FirstOrDefault(u => u.tx_hash_hex.Equals(pi.hash_hex));
                if (existingInput == null) 
                {
                    TransactionInputDTO newInput = new TransactionInputDTO();
                    newInput.output_index = pi.output_index;
                    newInput.address = pi.address;
                    newInput.tx_hash_hex = pi.hash_hex;
                    newInput.is_collateral = pi.is_collateral;
                    newInput.is_reference = pi.is_reference;
                    newInput.data_hash = pi.data_hash;
                    newInput.inline_datum_cbor = pi.inline_datum_cbor;
                    newInput.reference_script_hash = pi.reference_script_hash;
                    newInput.amounts = new List<TransactionAmountDTO>();

                    TransactionAmountDTO lovelace = new TransactionAmountDTO();
                    lovelace.value = pi.lovelace_value;
                    lovelace.unit = "lovelace";

                    newInput.amounts.Add(lovelace);

                    if (pi.asset_fingerprint != null) 
                    {
                        TransactionAmountDTO asset = new TransactionAmountDTO();
                        asset.value = pi.asset_quantity;
                        asset.unit = pi.asset_name;
                        asset.asset_fingerprint = pi.asset_fingerprint;

                        newInput.amounts.Add(asset);
                    }

                    inputs.Add(newInput);
                } else {
                    if (pi.asset_fingerprint != null) 
                    {
                        TransactionAmountDTO asset = new TransactionAmountDTO();
                        asset.value = pi.asset_quantity;
                        asset.unit = pi.asset_name;
                        asset.asset_fingerprint = pi.asset_fingerprint;

                        existingInput.amounts.Add(asset);
                    }
                }
            }

            transaction.inputs = inputs;

            return Ok(transaction);
        }

        /// <summary>Stake address certificate transactions.</summary>
        /// <remarks>Returns details of a transaction used to register a stake address given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/stake_address_registrations")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Certificates" })]
        public async Task<ActionResult<TransactionStakeAddressDTO>> GetTransactionStakeAddressRegistration(string transaction_hash)
        {
            if (
                _context.StakeRegistration == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null ||
                _context2.StakeRegistration == null ||
                _context2.StakeAddress == null ||
                _context2.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            Task<TransactionStakeAddressDTO?> t_registration = Task<TransactionStakeAddressDTO>.Run(() =>
            {
                var stake_registration = (
                    from sr in _context.StakeRegistration
                    join sa in _context.StakeAddress on sr.addr_id equals sa.id
                    join tx in _context.Transaction on sr.tx_id equals tx.id
                    where tx.hash == Convert.FromHexString(transaction_hash)
                    select new TransactionStakeAddressDTO()
                    {
                        cert_index = sr.cert_index,
                        epoch_no = sr.epoch_no,
                        stake_address = sa.view,
                        script_hash_hex = sa.script_hash_hex,
                        is_registration = true
                    }).FirstOrDefault();

                return stake_registration;
            });


            Task<TransactionStakeAddressDTO?> t_deregistration = Task<TransactionStakeAddressDTO>.Run(() =>
            {
                var stake_deregistration = (
                    from sr in _context2.StakeDeregistration
                    join sa in _context2.StakeAddress on sr.addr_id equals sa.id
                    join tx in _context2.Transaction on sr.tx_id equals tx.id
                    where tx.hash == Convert.FromHexString(transaction_hash)
                    select new TransactionStakeAddressDTO()
                    {
                        cert_index = sr.cert_index,
                        epoch_no = sr.epoch_no,
                        stake_address = sa.view,
                        script_hash_hex = sa.script_hash_hex,
                        is_registration = false
                    }).FirstOrDefault();

                return stake_deregistration;
            });

            Task.WaitAll(t_registration, t_deregistration);

            // get results from these tasks
            if (t_registration.Result !=null)
                return Ok(t_registration.Result);
            else if (t_deregistration.Result !=null)
                return Ok(t_deregistration.Result);

            return NotFound();
        }

        /// <summary>Stake address delegation transactions.</summary>
        /// <remarks>Returns details of a transaction used to delegate a given stake address to a pool.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/stake_address_delegations")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Certificates" })]
        public async Task<ActionResult<TransactionStakeAddressDelegationDTO>> GetTransactionStakeAddressDelegation(string transaction_hash)
        {
            if (
                _context.Delegation == null ||
                _context.StakeAddress == null ||
                _context.PoolHash == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var stake_delegation = await (
                from de in _context.Delegation
                join sa in _context.StakeAddress on de.addr_id equals sa.id
                join ph in _context.PoolHash on de.pool_hash_id equals ph.id
                join tx in _context.Transaction on de.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionStakeAddressDelegationDTO()
                {
                    cert_index = de.cert_index,
                    active_epoch_no = de.active_epoch_no,
                    stake_address = sa.view,
                    pool_hash_bech32 = ph.view,
                    pool_hash_hex = ph.hash_hex
                }).SingleOrDefaultAsync();

            if (stake_delegation == null)
            {
                return NotFound();
            }

            return Ok(stake_delegation);
        }

        /// <summary>Reward account withdrawal transactions.</summary>
        /// <remarks>Returns details of a transaction used to withdraw rewards given its staked address.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/withdrawals")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Withdrawals" })]
        public async Task<ActionResult<IEnumerable<TransactionStakeAddressWithdrawalDTO>>> GetTransactionWithdrawal(string transaction_hash)
        {
            if (
                _context.Withdrawal == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var withdrawal = await (
                from w in _context.Withdrawal
                join sa in _context.StakeAddress on w.addr_id equals sa.id
                join tx in _context.Transaction on w.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionStakeAddressWithdrawalDTO()
                {
                    stake_address = sa.view,
                    amount = w.amount,
                    redeemer_id = w.redeemer_id
                }).ToListAsync();

            if (withdrawal == null)
            {
                return NotFound();
            }
            
            return Ok(withdrawal);
        }

        /// <summary>Transactions for treasury payments to a stake address.</summary>
        /// <remarks>Returns details of a transaction used for payments between the treasury and a stake address.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/treasury")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Pots" })]
        public async Task<ActionResult<IEnumerable<TransactionTreasuryDTO>>> GetTransactionTreasury(string transaction_hash)
        {
            if (
                _context.Treasury == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var treasury = await (
                from t in _context.Treasury
                join sa in _context.StakeAddress on t.addr_id equals sa.id
                join tx in _context.Transaction on t.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionTreasuryDTO()
                {
                    cert_index = t.cert_index,
                    stake_address = sa.view,
                    amount = t.amount
                }).ToListAsync();

            if (treasury == null)
            {
                return NotFound();
            }
            
            return Ok(treasury);
        }

        /// <summary>Transactions for reserves payments to a stake address.</summary>
        /// <remarks>Returns details of a transaction used for payments between the reserves and a stake address.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/reserves")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Pots" })]
        public async Task<ActionResult<IEnumerable<TransactionTreasuryDTO>>> GetTransactionReserve(string transaction_hash)
        {
            if (
                _context.Reserve == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var reserve = await (
                from r in _context.Reserve
                join sa in _context.StakeAddress on r.addr_id equals sa.id
                join tx in _context.Transaction on r.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionTreasuryDTO()
                {
                    cert_index = r.cert_index,
                    stake_address = sa.view,
                    amount = r.amount
                }).ToListAsync();

            if (reserve == null)
            {
                return NotFound();
            }
            
            return Ok(reserve);
        }

        /// <summary>Transactions for block chain parameter change proposals.</summary>
        /// <remarks>Returns details of a transaction used for block chain parameter change proposals.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/param_proposals")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Blockchain" })]
        public async Task<ActionResult<IEnumerable<ParamProposal>>> GetTransactionParamProposal(string transaction_hash)
        {
            if (
                _context.ParamProposal == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var param_proposal = await (
                from pp in _context.ParamProposal
                join tx in _context.Transaction on pp.registered_tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select pp).ToListAsync();

            if (param_proposal == null)
            {
                return NotFound();
            }
            
            return Ok(param_proposal);
        }

        /// <summary>Pool retirement transactions.</summary>
        /// <remarks>Returns details of a transaction used to retire a stake pool.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/retiring_pools")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Certificates" })]
        public async Task<ActionResult<TransactionRetiringPoolDTO>> GetTransactionRetiringPool(string transaction_hash)
        {
            if (
                _context.PoolRetire == null ||
                _context.PoolHash == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var stake_delegation = await (
                from pr in _context.PoolRetire
                join ph in _context.PoolHash on pr.hash_id equals ph.id
                join tx in _context.Transaction on pr.announced_tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionRetiringPoolDTO()
                {
                    cert_index = pr.cert_index,
                    pool_hash_bech32 = ph.view,
                    pool_hash_hex = ph.hash_hex,
                    retiring_epoch = pr.retiring_epoch
                }).SingleOrDefaultAsync();

            if (stake_delegation == null)
            {
                return NotFound();
            }

            return Ok(stake_delegation);
        }

        /// <summary>On-chain pool update transactions.</summary>
        /// <remarks>Returns details of a transaction used to update a stake pool.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/updating_pools")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Certificates" })]
        public async Task<ActionResult<IEnumerable<TransactionUpdatingPoolDTO>>> GetTransactionUpdatingPool(string transaction_hash)
        {
            if (
                _context.PoolUpdate == null ||
                _context.Transaction == null ||
                _context.PoolHash == null ||
                _context.StakeAddress == null ||
                _context.PoolOwner == null ||
                _context.PoolMetadata == null || 
                _context.PoolOfflineData == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var pool_updates = await (
                from pu in _context.PoolUpdate
                join tx in _context.Transaction on pu.registered_tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select pu.id
                ).ToListAsync();

            if (pool_updates == null)
            {
                return NotFound();
            }

            List<TransactionUpdatingPoolDTO> pool_updates_combined = new List<TransactionUpdatingPoolDTO>();

            foreach (long pui in pool_updates)
            {

                Task<TransactionUpdatingPoolDTO?> t_pool_update_details = Task<TransactionUpdatingPoolDTO>.Run(() =>
                {
                    var pool_update_details = (
                        from pu in _context.PoolUpdate
                        join ph in _context.PoolHash on pu.hash_id equals ph.id
                        join sa in _context.StakeAddress on pu.reward_addr_id equals sa.id
                        where pu.id == pui
                        select new TransactionUpdatingPoolDTO()
                        {
                            cert_index = pu.cert_index,
                            pool_hash_bech32 = ph.view,
                            pool_hash_hex = ph.hash_hex,
                            vrf_key_hash_hex = pu.vrf_key_hash_hex,
                            reward_addr_hash_hex = sa.hash_hex,
                            pledge = pu.pledge,
                            margin = pu.margin,
                            fixed_cost = pu.fixed_cost,
                            active_epoch_no = pu.active_epoch_no
                        }).SingleOrDefault();

                    if (pool_update_details != null)
                    {
                        var owners_addresses = (
                            from po in _context.PoolOwner
                            join sa in _context.StakeAddress on po.addr_id equals sa.id
                            where po.pool_update_id == pui
                            select sa.view).ToList();

                        pool_update_details.owners_addresses = owners_addresses;
                    }

                    return pool_update_details;
                });

                Task<PoolOfflineDataDTO?> t_pool_offline_data = Task<PoolOfflineDataDTO>.Run(() =>
                {
                    var pool_offline_data = (
                        from pu in _context2.PoolUpdate
                        join pmr in _context2.PoolMetadata on pu.meta_id equals pmr.id
                        join pod in _context2.PoolOfflineData on pmr.id equals pod.pmr_id
                        where pu.id == pui
                        select new PoolOfflineDataDTO()
                        {
                            ticker_name = pod.ticker_name,
                            url = pmr.url,
                            hash_hex = pmr.hash_hex,
                            json = pod.json
                        }).SingleOrDefault();

                    return pool_offline_data;
                });

                Task<List<PoolRelayDTO>> t_pool_relays = Task<List<PoolRelayDTO>>.Run(() =>
                {
                    var pool_relays = (
                        from pr in _context3.PoolRelay
                        where pr.update_id == pui
                        select new PoolRelayDTO()
                        {
                            ipv4 = pr.ipv4,
                            ipv6 = pr.ipv6,
                            dns_name = pr.dns_name,
                            dns_srv_name = pr.dns_srv_name,
                            port = pr.port
                        }).ToList();

                    return pool_relays;
                });

                Task.WaitAll(t_pool_update_details, t_pool_offline_data, t_pool_relays);

                // get results from these tasks
                var pool_update_full = t_pool_update_details.Result;
                if (pool_update_full != null && t_pool_relays != null)
                {
                    pool_update_full.relays = t_pool_relays.Result;
                }
                if (pool_update_full != null && t_pool_offline_data != null && t_pool_offline_data.Result != null)
                {
                    pool_update_full.offline_data = t_pool_offline_data.Result;
                }

                pool_updates_combined.Add(pool_update_full);
            }

            return Ok(pool_updates_combined);
        }

        /// <summary>Metadata attached to a transaction.</summary>
        /// <remarks>Returns the metadata attached to a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/metadata")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Metadata" })]
        public async Task<ActionResult<IEnumerable<TransactionMetadataDTO>>> GetTransactionMetadata(string transaction_hash)
        {
            if (
                _context.TransactionMetadata == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var metadata = await (
                from tm in _context.TransactionMetadata
                join tx in _context.Transaction on tm.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new TransactionMetadataDTO()
                {
                    key = tm.key,
                    json = tm.json
                }).ToListAsync();

            if (metadata == null)
            {
                return NotFound();
            }
            
            return Ok(metadata);
        }

        /// <summary>Multi-asset mint events attached to a transaction.</summary>
        /// <remarks>Returns the details of a multi-asset mint event attached to a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/assetmints")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Assets" })]
        public async Task<ActionResult<IEnumerable<MultiAssetTransactionMintDTO>>> GetTransactionAssetMint(string transaction_hash)
        {
            if (
                _context.MultiAssetTransactionMint == null ||
                _context.MultiAsset == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var asset_mint = await (
                from mtm in _context.MultiAssetTransactionMint
                join ma in _context.MultiAsset on mtm.ident equals ma.id
                join tx in _context.Transaction on mtm.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new MultiAssetTransactionMintDTO()
                {
                    quantity = mtm.quantity,
                    policy_hex = ma.policy_hex,
                    name = Encoding.Default.GetString(ma.name),
                    fingerprint = ma.fingerprint
                }).ToListAsync();

            if (asset_mint == null)
            {
                return NotFound();
            }
            
            return Ok(asset_mint);
        }

        /// <summary>Redeemers attached to a transaction.</summary>
        /// <remarks>Returns redeemers information attached to a transaction given its hash.</remarks>
        /// <param name="transaction_hash">The transaction hash.</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 20)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/redeemers")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Contracts" })]
        public async Task<ActionResult<IEnumerable<RedeemerDTO>>> GetTransactionRedeemer(string transaction_hash)
        {
            if (
                _context.Redeemer == null ||
                _context.RedeemerData == null ||
                _context.Transaction == null
                )
            {
                return NotFound();
            }
            try
            {
                byte[] _res = Convert.FromHexString(transaction_hash);
            }
            catch (Exception e)
            {
                return NotFound();
            }

            var asset_mint = await (
                from r in _context.Redeemer
                join rd in _context.RedeemerData on r.redeemer_data_id equals rd.id
                join tx in _context.Transaction on r.tx_id equals tx.id
                where tx.hash == Convert.FromHexString(transaction_hash)
                select new RedeemerDTO()
                {
                    unit_mem = r.unit_mem,
                    unit_steps = r.unit_steps,
                    fee = r.fee,
                    purpose = r.purpose,
                    index = r.index,
                    script_hash_hex = r.script_hash_hex,
                    hash_hex = rd.hash_hex,
                    data_json = rd.value,
                    data_cbor = rd.bytes_hex
                }).ToListAsync();

            if (asset_mint == null)
            {
                return NotFound();
            }
            
            return Ok(asset_mint);
        }

    }
}
