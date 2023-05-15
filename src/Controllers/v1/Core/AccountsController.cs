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
// using Newtonsoft.Json;

namespace ApiCore.Controllers
{
    [ApiController]
    [Authorize(Policy="core-read")]
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
        [SwaggerOperation(Tags = new []{"Core", "Accounts"})]
        public async Task<ActionResult<AccountInfoDTO>> GetAccount(string stake_address)
        {
          if (_context.AccountCache == null)
          {
              return NotFound();
          }
            var account = await (
                from  aac in _context.AccountCache
                where aac.stake_address == stake_address
                select new AccountInfoDTO()
                {
                    stake_address = aac.stake_address,
                    is_registered = aac.is_registered,
                    last_reg_dereg_tx = aac.last_reg_dereg_tx,
                    last_reg_dereg_epoch_no = aac.last_reg_dereg_epoch_no,
                    pool_id = aac.pool_id,
                    last_deleg_tx = aac.last_deleg_tx,
                    delegated_since_epoch_no = aac.delegated_since_epoch_no,
                    total_balance = aac.total_balance,
                    controlled_stakes = aac.utxo,
                    total_rewards = aac.rewards,
                    total_withdrawals = aac.withdrawals,
                    available_rewards = aac.rewards_available
                }).SingleOrDefaultAsync();

            if (account == null)
            {
                return NotFound();
            }

            return account;
        }


        /// <summary>Rewards history.</summary>
        /// <remarks>Returns the earned rewards history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/rewards")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Rewards"})]
        public  async Task<IActionResult> GetAccountRewards(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.Reward == null ||
                _context.StakeAddress == null || 
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            // _logger.LogInformation(@$"AssetsController.GetAssetHistory: orderDir {orderDir}, 
            //         mintEventCount {mintEventCount}, 
            //         pageSize {pageSize}, 
            //         maxPageNo {maxPageNo},
            //         pageNo {pageNo}");

            IEnumerable<AccountRewardDTO> history = null;

            if (orderDir == "desc") 
            {
                history = await (
                    from r in _context.Reward
                    join sa in _context.StakeAddress on r.addr_id equals sa.id
                    join ph in _context.PoolHash on r.pool_id equals ph.id
                    where sa.view == stake_address
                    orderby r.earned_epoch descending
                    select new AccountRewardDTO()
                    {
                        earned_epoch = r.earned_epoch,
                        spendable_epoch = r.spendable_epoch,
                        type = r.type,
                        pool_id_hex = ph.hash_hex,
                        amount = r.amount
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                history = await (
                    from r in _context.Reward
                    join sa in _context.StakeAddress on r.addr_id equals sa.id
                    join ph in _context.PoolHash on r.pool_id equals ph.id
                    where sa.view == stake_address
                    orderby r.earned_epoch ascending
                    select new AccountRewardDTO()
                    {
                        earned_epoch = r.earned_epoch,
                        spendable_epoch = r.spendable_epoch,
                        amount = r.amount,
                        type = r.type,
                        pool_id_hex = ph.hash_hex
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            // Log the results before sending to the client
            // foreach (var item in history)
            // {
            //     Console.WriteLine($"Earned Epoch: {item.earned_epoch}, Amount: {item.amount}");
            //     _logger.LogInformation(@$"Earned Epoch: {item.earned_epoch}, Amount: {item.amount}");
            // }

            if (history == null)
            {
                return NotFound();
            }

            // // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Serialize the history object to a JSON string using Newtonsoft.Json
            // var jsonString = JsonConvert.SerializeObject(history);

            // Log the raw serialized data
            // _logger.LogInformation("Serialized data: {jsonString}", jsonString);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Account staking history.</summary>
        /// <remarks>Returns the staking history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/staking")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Staking"})]
        public async Task<IActionResult> GetAccountStaking(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.EpochStake == null ||
                _context.StakeAddress == null || 
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountStakingDTO> history = null;

            if (orderDir == "desc") 
            {
                history = await (
                    from es in _context.EpochStake
                    join sa in _context.StakeAddress on es.addr_id equals sa.id
                    join ph in _context.PoolHash on es.pool_id equals ph.id
                    where sa.view == stake_address
                    orderby es.epoch_no descending
                    select new AccountStakingDTO()
                    {
                        epoch_no = es.epoch_no,
                        amount = es.amount,
                        pool_id = ph.view
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                history = await (
                    from es in _context.EpochStake
                    join sa in _context.StakeAddress on es.addr_id equals sa.id
                    join ph in _context.PoolHash on es.pool_id equals ph.id
                    where sa.view == stake_address
                    orderby es.epoch_no ascending
                    select new AccountStakingDTO()
                    {
                        epoch_no = es.epoch_no,
                        amount = es.amount,
                        pool_id = ph.view
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (history == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Account delegation history.</summary>
        /// <remarks>Returns the delegation history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/delegations")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Delegations"})]
        public async Task<IActionResult> GetAccountDelegation(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.Reward == null ||
                _context.StakeAddress == null || 
                _context.PoolHash == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountDelegationDTO> history = null;

            if (orderDir == "desc") 
            {
                history = await (
                    from d in _context.Delegation
                    join sa in _context.StakeAddress on d.addr_id equals sa.id
                    join ph in _context.PoolHash on d.pool_hash_id equals ph.id
                    join tx in _context.Transaction on d.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby b.block_no descending
                    select new AccountDelegationDTO()
                    {
                        epoch_no = d.active_epoch_no,
                        tx_hash_hex = tx.hash_hex,
                        pool_id = ph.view,
                        slot_no = b.slot_no,
                        block_no = b.block_no,
                        block_time = b.time
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                history = await (
                    from d in _context.Delegation
                    join sa in _context.StakeAddress on d.addr_id equals sa.id
                    join ph in _context.PoolHash on d.pool_hash_id equals ph.id
                    join tx in _context.Transaction on d.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby b.block_no ascending
                    select new AccountDelegationDTO()
                    {
                        epoch_no = d.active_epoch_no,
                        tx_hash_hex = tx.hash_hex,
                        pool_id = ph.view,
                        slot_no = b.slot_no,
                        block_no = b.block_no,
                        block_time = b.time
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (history == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Account registration history.</summary>
        /// <remarks>Returns the registration history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/registrations")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Registrations"})]
        public async Task<IActionResult> GetAccountRegistration(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
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

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountRegistrationDTO> history = null;

            if (orderDir == "desc") 
            {
                history =  (
                    from sr in _context.StakeRegistration
                    join sa in _context.StakeAddress on sr.addr_id equals sa.id
                    join tx in _context.Transaction on sr.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby tx.id descending
                    select new
                    {
                        epoch_no = sr.epoch_no,
                        block_no = b.block_no,
                        tx_hash_hex = tx.hash_hex,
                        state = "registered"
                    }).AsEnumerable().Union(
                        from sd in _context.StakeDeregistration
                        join sa in _context.StakeAddress on sd.addr_id equals sa.id
                        join tx in _context.Transaction on sd.tx_id equals tx.id
                        join b in _context.Block on tx.block_id equals b.id
                        where sa.view == stake_address
                        orderby tx.id descending
                        select new
                        {
                            epoch_no = sd.epoch_no,
                            block_no = b.block_no,
                            tx_hash_hex = tx.hash_hex,
                            state = "deregistered"
                        })
                        .Select(x => new AccountRegistrationDTO
                        {
                            epoch_no = x.epoch_no,
                            block_no = x.block_no,
                            tx_hash_hex = x.tx_hash_hex,
                            state = x.state
                        }).OrderByDescending(x => x.block_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToList();
            } 
            else {
                history =  (
                    from sr in _context.StakeRegistration
                    join sa in _context.StakeAddress on sr.addr_id equals sa.id
                    join tx in _context.Transaction on sr.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby tx.id ascending
                    select new
                    {
                        epoch_no = sr.epoch_no,
                        block_no = b.block_no,
                        tx_hash_hex = tx.hash_hex,
                        state = "registered"
                    }).AsEnumerable().Union(
                        from sd in _context.StakeDeregistration
                        join sa in _context.StakeAddress on sd.addr_id equals sa.id
                        join tx in _context.Transaction on sd.tx_id equals tx.id
                        join b in _context.Block on tx.block_id equals b.id
                        where sa.view == stake_address
                        orderby tx.id ascending
                        select new
                        {
                            epoch_no = sd.epoch_no,
                            block_no = b.block_no,
                            tx_hash_hex = tx.hash_hex,
                            state = "deregistered"
                        })
                        .Select(x => new AccountRegistrationDTO
                        {
                            epoch_no = x.epoch_no,
                            block_no = x.block_no,
                            tx_hash_hex = x.tx_hash_hex,
                            state = x.state
                        }).OrderBy(x => x.block_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToList();
            }

            if (history == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Account withdrawal history.</summary>
        /// <remarks>Returns the withdrawal history from one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/withdrawals")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Withdrawals"})]
        public async Task<IActionResult> GetAccountWithdrawal(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.Withdrawal == null ||
                _context.StakeAddress == null || 
                _context.Transaction == null
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountWithdrawalDTO> history = null;

            if (orderDir == "desc") 
            {
                history = await (
                    from w in _context.Withdrawal
                    join sa in _context.StakeAddress on w.addr_id equals sa.id
                    join tx in _context.Transaction on w.tx_id equals tx.id
                    where sa.view == stake_address
                    orderby w.tx_id descending
                    select new AccountWithdrawalDTO()
                    {
                        tx_hash_hex = tx.hash_hex,
                        amount = w.amount
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                history = await (
                    from w in _context.Withdrawal
                    join sa in _context.StakeAddress on w.addr_id equals sa.id
                    join tx in _context.Transaction on w.tx_id equals tx.id
                    where sa.view == stake_address
                    orderby w.tx_id ascending
                    select new AccountWithdrawalDTO()
                    {
                        tx_hash_hex = tx.hash_hex,
                        amount = w.amount
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (history == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Move Instantaneous Rewards (MIR) history.</summary>
        /// <remarks>Returns the MIR history of one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/mirs")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "MIRs"})]
        public async Task<IActionResult> GetAccountMIR(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
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

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountMIRDTO> history = null;

            if (orderDir == "desc") 
            {
                history =  (
                    from t in _context.Treasury
                    join sa in _context.StakeAddress on t.addr_id equals sa.id
                    join tx in _context.Transaction on t.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby tx.id descending
                    select new
                    {
                        epoch_no = b.epoch_no,
                        block_no = b.block_no,
                        tx_hash_hex = tx.hash_hex,
                        amount = t.amount,
                        mir_type = "treasury"
                    }).AsEnumerable().Union(
                        from r in _context.Reserve
                        join sa in _context.StakeAddress on r.addr_id equals sa.id
                        join tx in _context.Transaction on r.tx_id equals tx.id
                        join b in _context.Block on tx.block_id equals b.id
                        where sa.view == stake_address
                        orderby tx.id descending
                        select new
                        {
                            epoch_no = b.epoch_no,
                            block_no = b.block_no,
                            tx_hash_hex = tx.hash_hex,
                            amount = r.amount,
                            mir_type = "reserve"
                        })
                        .Select(x => new AccountMIRDTO
                        {
                            epoch_no = x.epoch_no,
                            block_no = x.block_no,
                            tx_hash_hex = x.tx_hash_hex,
                            amount = x.amount,
                            mir_type = x.mir_type
                        }).OrderByDescending(x => x.block_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToList();
            } 
            else {
                history =  (
                    from t in _context.Treasury
                    join sa in _context.StakeAddress on t.addr_id equals sa.id
                    join tx in _context.Transaction on t.tx_id equals tx.id
                    join b in _context.Block on tx.block_id equals b.id
                    where sa.view == stake_address
                    orderby tx.id ascending
                    select new
                    {
                        epoch_no = b.epoch_no,
                        block_no = b.block_no,
                        tx_hash_hex = tx.hash_hex,
                        amount = t.amount,
                        mir_type = "treasury"
                    }).AsEnumerable().Union(
                        from r in _context.Reserve
                        join sa in _context.StakeAddress on r.addr_id equals sa.id
                        join tx in _context.Transaction on r.tx_id equals tx.id
                        join b in _context.Block on tx.block_id equals b.id
                        where sa.view == stake_address
                        orderby tx.id ascending
                        select new
                        {
                            epoch_no = b.epoch_no,
                            block_no = b.block_no,
                            tx_hash_hex = tx.hash_hex,
                            amount = r.amount,
                            mir_type = "reserve"
                        })
                        .Select(x => new AccountMIRDTO
                        {
                            epoch_no = x.epoch_no,
                            block_no = x.block_no,
                            tx_hash_hex = x.tx_hash_hex,
                            amount = x.amount,
                            mir_type = x.mir_type
                        }).OrderBy(x => x.block_no).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToList();
            }

            if (history == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(history);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }

        /// <summary>Account associated addresses.</summary>
        /// <remarks>Returns all addresses associated to one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/addresses")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Addresses"})]
        public async Task<IActionResult> GetAccountAddress(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (
                _context.TransactionOutput == null ||
                _context.StakeAddress == null 
                )
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountAddressDTO> addresses = null;

            if (orderDir == "desc") 
            {
                addresses = await (
                    from txo in _context.TransactionOutput
                    join sa in _context.StakeAddress on txo.stake_address_id equals sa.id
                    where sa.view == stake_address
                    orderby txo.address descending
                    select new AccountAddressDTO()
                    {
                        address = txo.address,
                        address_has_script = txo.address_has_script
                    }).Distinct().OrderByDescending(x => x.address).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                addresses = await (
                    from txo in _context.TransactionOutput
                    join sa in _context.StakeAddress on txo.stake_address_id equals sa.id
                    where sa.view == stake_address
                    orderby txo.address ascending
                    select new AccountAddressDTO()
                    {
                        address = txo.address,
                        address_has_script = txo.address_has_script
                    }).Distinct().OrderBy(x => x.address).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (addresses == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(addresses);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }


        /// <summary>Account assets holdings.</summary>
        /// <remarks>Returns all assets held by one account given its stake address.</remarks>
        /// <param name="stake_address">Bech32 Stake address</param>
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
        [HttpGet("api/core/accounts/{stake_address}/assets")]
        [SwaggerOperation(Tags = new []{"Core", "Accounts", "Assets"})]
        public async Task<IActionResult> GetAccountAsset(string stake_address, [FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
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

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);

            IEnumerable<AccountAssetDTO> assets = null;

            if (orderDir == "desc") 
            {
                assets = await (
                    from maac in _context.MultiAssetAddressCache
                    join ma in _context.MultiAsset on maac.asset_id equals ma.id
                    join sub in ((from txo in _context.TransactionOutput
                                    join sa in _context.StakeAddress on txo.stake_address_id equals sa.id
                                    where sa.view == stake_address
                                    select new { txo.address }
                                ).Distinct()) on maac.address equals sub.address
                    group new { ma, maac } by new { ma.policy, ma.fingerprint, ma.name } into g
                    orderby g.Key.name descending
                    select new AccountAssetDTO()
                    {
                        policy_hex = Convert.ToHexString(g.Key.policy).ToLower(),
                        fingerprint = g.Key.fingerprint,
                        name = g.Key.name != null ? Encoding.Default.GetString(g.Key.name) : "",
                        quantity = (ulong)g.Sum(b => (decimal)b.maac.quantity)
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                assets = await (
                    from maac in _context.MultiAssetAddressCache
                    join ma in _context.MultiAsset on maac.asset_id equals ma.id
                    join sub in ((from txo in _context.TransactionOutput
                                    join sa in _context.StakeAddress on txo.stake_address_id equals sa.id
                                    where sa.view == stake_address
                                    select new { txo.address }
                                ).Distinct()) on maac.address equals sub.address
                    group new { ma, maac } by new { ma.policy, ma.fingerprint, ma.name } into g
                    orderby g.Key.name ascending
                    select new AccountAssetDTO()
                    {
                        policy_hex = Convert.ToHexString(g.Key.policy).ToLower(),
                        fingerprint = g.Key.fingerprint,
                        name = g.Key.name != null ? Encoding.Default.GetString(g.Key.name) : "",
                        quantity = (ulong)g.Sum(b => (decimal)b.maac.quantity)
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (assets == null)
            {
                return NotFound();
            }

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(assets);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }
    }
}
