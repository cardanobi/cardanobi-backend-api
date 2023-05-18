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
using System.Text.Json.Nodes;
using Npgsql;

namespace ApiCore.Controllers
{
    [ApiController]
    // [Authorize(Policy="core-read")]
    [AllowAnonymous]
    [Produces("application/json")]
    public class PollsController : ControllerBase
    {
        private readonly cardanobiCoreContext _context;
        private readonly cardanobiCoreContext2 _context2;
        private readonly cardanobiCoreContext3 _context3;
        private readonly ILogger<PollsController> _logger;

        public PollsController(cardanobiCoreContext context, cardanobiCoreContext2 context2, cardanobiCoreContext3 context3, ILogger<PollsController> logger)
        {
            _context = context;
            _context2 = context2;
            _context3 = context3;
            _logger = logger;
        }

        /// <summary>All polls.</summary>
        /// <remarks>Returns the list of all polls defined on chain.</remarks>
        /// <param name="page_no">Page number to retrieve - defaults to 1</param>
        /// <param name="page_size">Number of results per page - defaults to 20 - max 100</param>
        /// <param name="order">Prescribes in which order results are returned - "desc" descending (default) from newest to oldest - "asc" ascending from oldest to newest</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/AccountCache/5
        [EnableQuery(PageSize = 100)]
        [HttpGet("api/core/polls")]
        [SwaggerOperation(Tags = new[] { "Core", "Polls" })]
        public async Task<ActionResult<PollOverviewDTO>> GetPoll([FromQuery] long? page_no, [FromQuery] long? page_size, [FromQuery] string? order)
        {
            if (_context.AccountCache == null)
            {
                return NotFound();
            }

            string orderDir = order == null ? "desc" : order;
            long pageSize = page_size == null ? 20 : Math.Min(100, Math.Max(1,(long)page_size));
            long pageNo = page_no == null ? 1 : Math.Max(1,(long)page_no);


            IEnumerable<PollOverviewDTO> polls = null;

            if (orderDir == "desc") 
            {
                polls = await (
                    from cp in _context.CBIPoll
                    join tm in _context.TransactionMetadata on cp.tx_id equals tm.tx_id
                    join tx in _context.Transaction on cp.tx_id equals tx.id
                    orderby cp.tx_id descending
                    select new PollOverviewDTO()
                    {
                        poll_hash = Convert.ToHexString(cp.question_hash).ToLower(),
                        tx_hash_hex = tx.hash_hex,
                        start_epoch_no = cp.start_epoch_no,
                        end_epoch_no = cp.end_epoch_no,
                        json = tm.json
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            } else {
                polls = await (
                    from cp in _context.CBIPoll
                    join tm in _context.TransactionMetadata on cp.tx_id equals tm.tx_id
                    join tx in _context.Transaction on cp.tx_id equals tx.id
                    orderby cp.tx_id ascending
                    select new PollOverviewDTO()
                    {
                        poll_hash = Convert.ToHexString(cp.question_hash).ToLower(),
                        tx_hash_hex = tx.hash_hex,
                        start_epoch_no = cp.start_epoch_no,
                        end_epoch_no = cp.end_epoch_no,
                        json = tm.json
                    }).Skip((int)((pageNo-1)*pageSize)).Take((int)pageSize).ToListAsync();
            }

            if (polls == null)
            {
                return NotFound();
            }

            // return Ok(polls);

            // Serialize the history object to a JSON string using System.Text.Json
            var jsonString = JsonSerializer.Serialize(polls);

            // Return the JSON string as a ContentResult with the appropriate content type
            // TODO this is temporary until we find out the reason for the result ordering to be messed up as soon as we include reward.amount in the response!
            return Content(jsonString, "application/json");
        }

        /// <summary>One poll full details by hash.</summary>
        /// <remarks>Returns on-chain information about a poll given the hash of its question.</remarks>
        /// <param name="poll_hash">The HEX encoding of the poll's hash (e.g. the hash of the poll's question).</param>
        /// <response code="200">OK: Successful request.</response>
        /// <response code="400">Bad Request: The request was unacceptable, often due to missing a required parameter.</response>
        /// <response code="401">Unauthorized: No valid API key provided.</response>
        /// <response code="402">Quota Exceeded: This API key has reached its usage limit on request.</response>
        /// <response code="403">Access Denied: The request is missing a valid API key or token.</response>
        /// <response code="404">Not Found: The requested resource cannot be found.</response>
        /// <response code="429">Too Many Requests: This API key has reached its rate limit.</response>
        // GET: api/AccountCache/5
        [EnableQuery(PageSize = 1)]
        [HttpGet("api/core/polls/{poll_hash:regex(^[[a-fA-F0-9]]{{64}}$)}")]
        [SwaggerOperation(Tags = new []{"Core", "Polls"})]
        public async Task<ActionResult<PollDTO>> GetPoll(string poll_hash)
        {
            if (_context.AccountCache == null)
            {
                return NotFound();
            }

            Task<PollDTO?> t_poll = Task<PollDTO>.Run(() =>
            {
                var poll = (
                    from cp in _context.CBIPoll
                    join tm in _context.TransactionMetadata on cp.tx_id equals tm.tx_id
                    join tx in _context.Transaction on tm.tx_id equals tx.id
                    where cp.question_hash == Convert.FromHexString(poll_hash)
                    select new PollDTO()
                    {
                        tx_hash_hex = tx.hash_hex,
                        poll_hash = cp.question_hash_hex,
                        start_epoch_no = cp.start_epoch_no,
                        end_epoch_no = cp.end_epoch_no,
                        json = tm.json
                    }).SingleOrDefaultAsync();

                return poll;
            });

            Task<List<PollVoteDTO>> t_votes = Task<List<PollVoteDTO>>.Run(() =>
            {
                string sql = $@"
                WITH myconstants (poll_end_epoch_no, current_epoch_no) as (
                    select (select end_epoch_no from _cbi_polls where question_hash = '\x{poll_hash}'::hash32type::bytea), (select max(no) from epoch)
                )   
                select 
                    pod.ticker_name, cpp.pool_id,
                    b.epoch_no as epoch_no_vote,
                    encode(tx.hash::bytea, 'hex') as tx_hash_hex,
                    encode(ekw.hash::bytea, 'hex') as extra_sign_hash, 
                    tm.json as response_json,
                    cpp.cold_vkey,
                    pod.json as pool_offline_data_json,
                    (select count(1) from _cbi_active_stake_cache_account casca
                        where casca.pool_id = cpp.pool_id 
                            and casca.epoch_no = least(poll_end_epoch_no, current_epoch_no)) as delegators_count,
                    (select coalesce(sum(casca.amount), 0) from _cbi_active_stake_cache_account casca
                    where casca.pool_id = cpp.pool_id 
                        and casca.epoch_no = least(poll_end_epoch_no, current_epoch_no)) as delegated_stakes
                from myconstants, tx_metadata tm
                inner join extra_key_witness ekw on ekw.tx_id = tm.tx_id
                inner join tx on tx.id=tm.tx_id
                inner join ""_cbi_pool_params"" cpp on cpp.cold_vkey = encode(ekw.hash::bytea, 'hex')
                inner join pool_hash ph on ph.""view"" = cpp.pool_id 
                inner join block b on b.id = tx.block_id 
                left join pool_offline_data pod on pod.pool_id = ph.id
                where tm.key ='94'
                and tm.json ->> '2' = @pollHashParameter::text
                and (pod.pmr_id is null or pod.pmr_id = (select max(pod2.pmr_id) from pool_offline_data pod2 where pod2.pool_id = ph.id))
                and (b.epoch_no < poll_end_epoch_no)
                order by tm.tx_id";

                // byte[] pollHashBytes = Encoding.Default.GetBytes(poll_hash);
                // var pollHashParameter = new NpgsqlParameter<byte[]>("@pollHash", pollHashBytes);
                var pollHashParameter2 = new NpgsqlParameter<string>("@pollHashParameter", "0x" + poll_hash);

                var votes = _context2.PollVoteDTO.FromSqlRaw(sql, pollHashParameter2).ToList();

                return votes;
            });


            Task.WaitAll(t_poll, t_votes);

            if (t_poll.Result == null)
            {
                return NotFound();
            }

            // Processing the poll
            // Create a JsonNode DOM from a JSON string.
            JsonNode jNode = JsonNode.Parse(t_poll.Result.json);
            string question = jNode["0"].ToJsonString().Replace("\"", "").Replace(",", "").Replace("[", "").Replace("]", "");
            string[] choices = jNode["1"].ToJsonString().Replace("\"", "").Replace(",", "").Replace("[[", "").Replace("]]", "").Split("][");
            t_poll.Result.question = question;
            t_poll.Result.choices = choices;
            t_poll.Result.votes = new List<PollVotePubDTO> ();

            // Processing the votes
            int voteCount = 0;
            int[] voteTally = new int[choices.Length];
            float[] voteTallyPct = new float[choices.Length];
    
            int delegatorCount = 0;
            int[] voteTallyDelegators = new int[choices.Length];
            float[] voteTallyDelegatorsPct = new float[choices.Length];

            ulong stakeCount = 0;
            ulong[] voteTallyStakes = new ulong[choices.Length];        
            float[] voteTallyStakesPct = new float[choices.Length];

            foreach (PollVoteDTO v in t_votes.Result)
            {
                PollVotePubDTO vote = new PollVotePubDTO();
                JsonNode jnPoolMeta = v.pool_offline_data_json == null ? null:JsonNode.Parse(v.pool_offline_data_json);
                JsonNode jnPollResponse = JsonNode.Parse(v.response_json);

                vote.ticker_name = v.ticker_name;
                vote.pool_name = jnPoolMeta == null ? "":jnPoolMeta["name"].ToJsonString().Replace("\"","");
                vote.pool_id = v.pool_id;
                vote.tx_hash_hex = v.tx_hash_hex;
                int voteID = int.Parse(jnPollResponse["3"].ToJsonString());
                vote.response = choices[voteID];
                vote.response_json = v.response_json;
                vote.extra_sign_hash = v.extra_sign_hash;
                vote.cold_vkey = v.cold_vkey;
                vote.delegators_count = v.delegators_count;
                vote.delegated_stakes = v.delegated_stakes;

                voteCount++;
                voteTally[voteID]++;

                delegatorCount += vote.delegators_count;
                voteTallyDelegators[voteID] += vote.delegators_count;

                stakeCount += vote.delegated_stakes;
                voteTallyStakes[voteID] += vote.delegated_stakes;

                t_poll.Result.votes.Add(vote);
            }

            for( int k=0; k<choices.Length; k++) 
            {
                voteTallyPct[k] = (float)voteTally[k] / (float)voteCount;
                voteTallyDelegatorsPct[k] = (float)voteTallyDelegators[k] / (float)delegatorCount;
                voteTallyStakesPct[k] = (float)voteTallyStakes[k] / (float)stakeCount;
            }
            // Processing poll summaries
            PollSummaryDTO sumPoll = new PollSummaryDTO();

            PollSummaryVotesDTO sumVotes = new PollSummaryVotesDTO();
            sumVotes.total = voteCount;
            sumVotes.counts = new List<int>(voteTally);
            sumVotes.pcts = new List<float>(voteTallyPct);

            PollSummaryDelegatorsDTO sumDelegators = new PollSummaryDelegatorsDTO();
            sumDelegators.total = delegatorCount;
            sumDelegators.sums = new List<int>(voteTallyDelegators);
            sumDelegators.pcts = new List<float>(voteTallyDelegatorsPct);

            PollSummaryStakesDTO sumStakes = new PollSummaryStakesDTO();
            sumStakes.total = stakeCount;
            sumStakes.sums = new List<ulong>(voteTallyStakes);
            sumStakes.pcts = new List<float>(voteTallyStakesPct);

            sumPoll.votes = sumVotes;
            sumPoll.delegators = sumDelegators;
            sumPoll.stakes = sumStakes;

            t_poll.Result.summary = sumPoll;

            return Ok(t_poll.Result);
        }
    }
}
