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

namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy = "core-read")]
    [Produces("application/json")]
    public class AccountsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(cardanobiCoreContext context, ILogger<AccountsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>One account by stake address.</summary>
        /// <remarks>Returns on-chain information about an account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/AccountCache/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/accounts/{stake_address}")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts" })]
        public async Task<ActionResult<AccountInfoDTO>> GetAccount(string stake_address)
        {
            if (_context.AccountCache == null)
            {
                return NotFound();
            }

            var query = _context.AccountCache
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountInfoDTO
                {
                    stake_address = ps.StakeAddress.view,
                    is_registered = ps.is_registered,
                    last_reg_dereg_tx = ps.last_reg_dereg_tx,
                    last_reg_dereg_epoch_no = ps.last_reg_dereg_epoch_no,
                    pool_id = ps.PoolHash.view,
                    last_deleg_tx = ps.last_deleg_tx,
                    delegated_since_epoch_no = ps.delegated_since_epoch_no,
                    total_balance = ps.total_balance,
                    controlled_stakes = ps.utxo,
                    total_rewards = ps.rewards,
                    total_withdrawals = ps.withdrawals,
                    available_rewards = ps.rewards_available
                });

            var account = await query.FirstOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }


        /// <summary>Rewards history.</summary>
        /// <remarks>Returns the earned rewards history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// 
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=earned_epoch", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/rewards")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Rewards" })]
        public async Task<ActionResult<IEnumerable<AccountRewardDTO>>> GetAccountRewards(string stake_address)
        {
            if (
                _context.Reward == null ||
                _context.StakeAddress == null ||
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            var query = _context.Reward
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountRewardDTO
                {
                    earned_epoch = ps.earned_epoch,
                    spendable_epoch = ps.spendable_epoch,
                    type = ps.type,
                    pool_id_hex = ps.PoolHash.hash_hex,
                    amount = ps.amount
                });

            var history = await query.ToListAsync();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Account staking history.</summary>
        /// <remarks>Returns the staking history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=epoch_no", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/staking")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Staking" })]
        public async Task<ActionResult<IEnumerable<AccountStakingDTO>>> GetAccountStaking(string stake_address)
        {
            if (
                _context.EpochStake == null ||
                _context.StakeAddress == null ||
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            var query = _context.EpochStake
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.PoolHash)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountStakingDTO
                {
                    epoch_no = ps.epoch_no,
                    amount = ps.amount,
                    pool_id = ps.PoolHash.view
                });

            var history = await query.ToListAsync();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Account delegation history.</summary>
        /// <remarks>Returns the delegation history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=block_no", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/delegations")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Delegations" })]
        public async Task<ActionResult<IEnumerable<AccountDelegationDTO>>> GetAccountDelegation(string stake_address)
        {
            if (
                _context.Reward == null ||
                _context.StakeAddress == null ||
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            var query = _context.Delegation
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.PoolHash)
                .Include(ps => ps.Transaction)
                    .ThenInclude(ps => ps.Block)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountDelegationDTO
                {
                    epoch_no = ps.active_epoch_no,
                    tx_hash_hex = ps.Transaction.hash_hex,
                    pool_id = ps.PoolHash.view,
                    slot_no = ps.Transaction.Block.slot_no,
                    block_no = ps.Transaction.Block.block_no,
                    block_time = ps.Transaction.Block.time
                });

            var history = await query.ToListAsync();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Account registration history.</summary>
        /// <remarks>Returns the registration history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=block_no", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/registrations")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Registrations" })]
        public async Task<ActionResult<IEnumerable<AccountRegistrationDTO>>> GetAccountRegistration(string stake_address)
        {
            if (
                _context.StakeRegistration == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null ||
                _context.Block == null
                )
            {
                return NotFound();
            }

            var query = _context.StakeRegistration
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.Transaction)
                    .ThenInclude(ps => ps.Block)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new 
                {
                    epoch_no = ps.epoch_no,
                    block_no = ps.Transaction.Block.block_no,
                    tx_hash_hex = ps.Transaction.hash_hex,
                    state = "registered"
                })
                .AsEnumerable().Union(
                    _context.StakeDeregistration
                        .Include(ps => ps.StakeAddress)
                        .Include(ps => ps.Transaction)
                            .ThenInclude(ps => ps.Block)
                        .Where(ps => ps.StakeAddress.view == stake_address)
                        .Select(ps => new 
                        {
                            epoch_no = ps.epoch_no,
                            block_no = ps.Transaction.Block.block_no,
                            tx_hash_hex = ps.Transaction.hash_hex,
                            state = "deregistered"
                        })
                )
                .Select(ps => new AccountRegistrationDTO
                {
                    epoch_no = ps.epoch_no,
                    block_no = ps.block_no,
                    tx_hash_hex = ps.tx_hash_hex,
                    state = ps.state
                });
                
            var history = query.ToList();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Account withdrawal history.</summary>
        /// <remarks>Returns the withdrawal history from one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=block_no", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/withdrawals")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Withdrawals" })]
        public async Task<ActionResult<IEnumerable<AccountWithdrawalDTO>>> GetAccountWithdrawal(string stake_address)
        {
            if (
                _context.Withdrawal == null ||
                _context.StakeAddress == null ||
                _context.StakeAddress == null ||
                _context.Block == null
                )
            {
                return NotFound();
            }

            var query = _context.Withdrawal
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.Transaction)
                    .ThenInclude(ps => ps.Block)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountWithdrawalDTO
                {
                    block_no = ps.Transaction.Block.block_no,
                    tx_hash_hex = ps.Transaction.hash_hex,
                    amount = ps.amount
                });

            var history = await query.ToListAsync();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Move Instantaneous Rewards (MIR) history.</summary>
        /// <remarks>Returns the MIR history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [CustomEnableQueryAttribute("$orderby=block_no", PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/mirs")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "MIRs" })]
        public async Task<ActionResult<IEnumerable<AccountMIRDTO>>> GetAccountMIR(string stake_address)
        {
            if (
                _context.Treasury == null ||
                _context.Reserve == null ||
                _context.StakeAddress == null ||
                _context.Transaction == null ||
                _context.Block == null
                )
            {
                return NotFound();
            }

            var query = _context.Treasury
                .Include(ps => ps.StakeAddress)
                .Include(ps => ps.Transaction)
                    .ThenInclude(ps => ps.Block)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new 
                {
                    epoch_no = ps.Transaction.Block.epoch_no,
                    block_no = ps.Transaction.Block.block_no,
                    tx_hash_hex = ps.Transaction.hash_hex,
                    amount = ps.amount,
                    mir_type = "treasury"
                })
                .AsEnumerable().Union(
                    _context.Reserve
                        .Include(ps => ps.StakeAddress)
                        .Include(ps => ps.Transaction)
                            .ThenInclude(ps => ps.Block)
                        .Where(ps => ps.StakeAddress.view == stake_address)
                        .Select(ps => new 
                        {
                            epoch_no = ps.Transaction.Block.epoch_no,
                            block_no = ps.Transaction.Block.block_no,
                            tx_hash_hex = ps.Transaction.hash_hex,
                            amount = ps.amount,
                            mir_type = "reserve"
                        })
                )
                .Select(ps => new AccountMIRDTO
                {
                    epoch_no = ps.epoch_no,
                    block_no = ps.block_no,
                    tx_hash_hex = ps.tx_hash_hex,
                    amount = ps.amount,
                    mir_type = ps.mir_type
                });
                
            var history = query.ToList();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }

        /// <summary>Account associated addresses.</summary>
        /// <remarks>Returns all addresses associated to one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/addresses")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Addresses" })]
        public async Task<ActionResult<IEnumerable<AccountAddressDTO>>> GetAccountAddress(string stake_address)
        {
            if (
                _context.TransactionOutput == null ||
                _context.StakeAddress == null
                )
            {
                return NotFound();
            }

            var query = _context.TransactionOutput
                .Include(ps => ps.StakeAddress)
                .Where(ps => ps.StakeAddress.view == stake_address)
                .Select(ps => new AccountAddressDTO
                {
                    address = ps.address,
                    address_has_script = ps.address_has_script
                });

            var history = await query.ToListAsync();

            if (history == null || history.Count == 0)
            {
                return NotFound();
            }

            return history;
        }


        /// <summary>Account assets holdings.</summary>
        /// <remarks>Returns all assets held by one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/Block
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/accounts/{stake_address}/assets")]
        [SwaggerOperation(Tags = new[] { "Core", "Accounts", "Assets" })]
        public async Task<ActionResult<IEnumerable<AccountAssetDTO>>> GetAccountAsset(string stake_address)
        {
            if (
                _context.MultiAssetAddressCache == null ||
                _context.MultiAsset == null ||
                _context.TransactionOutput == null ||
                _context.StakeAddress == null
                )
            {
                return NotFound();
            }

            var subQuery = _context.TransactionOutput
                .Include(txo => txo.StakeAddress)
                .Where(txo => txo.StakeAddress.view == stake_address)
                .Select(txo => new { txo.address })
                .Distinct();

            var results = await _context.MultiAssetAddressCache
                .Join(
                    _context.MultiAsset,
                    maac => maac.asset_id,
                    ma => ma.id,
                    (maac, ma) => new { maac, ma }
                )
                .Join(
                    subQuery,
                    joint => joint.maac.address,
                    sub => sub.address,
                    (joint, sub) => new { joint.maac, joint.ma }
                )
                .GroupBy(
                    joint => new { joint.ma.policy, joint.ma.fingerprint, joint.ma.name },
                    joint => new { joint.ma, joint.maac }
                )
                .Select(g => new AccountAssetDTO
                {
                    policy_hex = Convert.ToHexString(g.Key.policy).ToLower(),
                    fingerprint = g.Key.fingerprint,
                    name = g.Key.name != null ? Encoding.Default.GetString(g.Key.name) : "",
                    quantity = (ulong)g.Sum(b => (decimal)b.maac.quantity)
                })
                .ToListAsync();

            if (results == null || results.Count == 0)
            {
                return NotFound();
            }

            return results;
        }
    }
}
