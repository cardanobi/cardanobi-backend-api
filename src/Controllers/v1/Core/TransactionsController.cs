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
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
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
                        lovelace_value = g.Sum(b => b.txo.value),
                        asset_quantity = g.Sum(b => b.mtog.quantity)
                    }).ToList();

                // Preparing the amounts summary (total lovelace value + total multi asset values )
                List<TransactionAmountDTO> amounts = new List<TransactionAmountDTO>();
                decimal lovelaceValue = 0;
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
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
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
                from tx in _context.Transaction
                join b in _context.Block on tx.block_id equals b.id
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
                    from txo in _context.TransactionOutput
                    join da in _context.Datum on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context.Script on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context.MultiAssetTransactionOutput on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context.MultiAsset on mtog.ident equals ma.id into maGroup
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
                    from cto in _context2.CollateralTransactionOutput
                    join da in _context2.Datum on cto.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context2.Script on cto.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context2.MultiAssetTransactionOutput on cto.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context2.MultiAsset on mtog.ident equals ma.id into maGroup
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
                    where txo.index == tx_in.tx_out_index && tx.id == transaction.id
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
                    from txo in _context2.TransactionOutput
                    join ctx_in in _context2.CollateralTransactionInput on txo.tx_id equals ctx_in.tx_out_id
                    join tx in _context2.Transaction on txo.tx_id equals tx.id
                    join da in _context2.Datum on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context2.Script on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context2.MultiAssetTransactionOutput on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context2.MultiAsset on mtog.ident equals ma.id into maGroup
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
                    from txo in _context3.TransactionOutput
                    join rtx_in in _context3.ReferenceTransactionInput on txo.tx_id equals rtx_in.tx_out_id
                    join tx in _context3.Transaction on txo.tx_id equals tx.id
                    join da in _context3.Datum on txo.inline_datum_id equals da.id into daGroup
                    from dag in daGroup.DefaultIfEmpty()
                    join sc in _context3.Script on txo.reference_script_id equals sc.id into scGroup
                    from scg in scGroup.DefaultIfEmpty()
                    join mto in _context3.MultiAssetTransactionOutput on txo.id equals mto.tx_out_id into mtoGroup
                    from mtog in mtoGroup.DefaultIfEmpty()
                    join ma in _context3.MultiAsset on mtog.ident equals ma.id into maGroup
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

            // get results from these
            var main_inputs = t_main_inputs.Result;
            var collateral_inputs = t_collateral_inputs.Result;
            var reference_inputs = t_reference_inputs.Result;

            // Preparing the inputs
            List<TransactionInputDTO> inputs = new List<TransactionInputDTO>();

            foreach (TransactionInputProjectionDTO pi in main_inputs.Concat(collateral_inputs).Concat(reference_inputs)) 
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
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        // GET: api/Block/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/transactions/{transaction_hash:length(64)}/stake_address_certs")]
        [SwaggerOperation(Tags = new[] { "Core", "Transactions", "Certificates" })]
        public async Task<ActionResult<TransactionDTO>> GetTransactionStakeAddressCert(string transaction_hash)
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

            Task<TransactionStakeAddressDTO> t_registration = Task<TransactionStakeAddressDTO>.Run(() =>
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
                    }).Single();

                return stake_registration;
            });


            Task<TransactionStakeAddressDTO> t_deregistration = Task<TransactionStakeAddressDTO>.Run(() =>
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
                    }).Single();

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

    }
}
