using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class PollOverviewDTO
    {
        /// <summary>The HEX encoding of the poll's question (e.g. the poll's hash).</summary>
        public string poll_hash { get; set; }

        /// <summary>The HEX encoding of the transaction that created the poll.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The epoch number marking the start of this poll.</summary>
        public int start_epoch_no { get; set; }

        /// <summary>The epoch number marking the end of this poll.</summary>
        public int end_epoch_no { get; set; }

        /// <summary>The poll's on-chain JSON payload, containing questions and possible answers.</summary>
        public string json { get; set; }
    }
    public partial class PollDTO
    {
        /// <summary>The HEX encoding of the poll's question (e.g. the poll's hash).</summary>
        public string poll_hash { get; set; }

        /// <summary>The the poll's question.</summary>
        public string question  { get; set; }

        /// <summary>The the poll's possible answer choices.</summary>
        public string[] choices  { get; set; }

        /// <summary>The HEX encoding of the transaction that created the poll.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The epoch number marking the start of this poll.</summary>
        public int start_epoch_no { get; set; }

        /// <summary>The epoch number marking the end of this poll.</summary>
        public int end_epoch_no { get; set; }

        /// <summary>The poll's on-chain JSON payload, containing questions and possible answers.</summary>
        public string json { get; set; }

        /// <summary>The poll summary results.</summary>
        public PollSummaryDTO summary { get; set; }
        
        /// <summary>The list of votes.</summary>
        public List<PollVotePubDTO> votes { get; set; } = null!;
    }

    [NotMapped]
    public partial class PollVoteDTO
    {
        /// <summary>The pool's ticker name.</summary>
        public string? ticker_name { get; set; }

        /// <summary>The Bech32 encoding of the pool hash for the pool that cast this vote.</summary>
        public string pool_id { get; set; }

        /// <summary>The epoch number when this vote took place.</summary>
        public int epoch_no_vote { get; set; }

        /// <summary>The HEX encoding of the transaction carrying this vote.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The vote's JSON payload.</summary>
        public string response_json { get; set; }

        /// <summary>The HEX encoding of the extra signer hash attached to the vote transaction.</summary>
        public string extra_sign_hash { get; set; }

        /// <summary>The hexadecimal encoding of the pool verification key hash for the pool that cast this vote.</summary>
        public string cold_vkey { get; set; }
        
        /// <summary>The pool's offline data as JSON.</summary>
        public string? pool_offline_data_json { get; set; }

        /// <summary>The number of delegators currently delegating to this pool.</summary>
        public int delegator_count { get; set; }

        /// <summary>The current delegated stakes for this pool (in Lovelace).</summary>
        public ulong delegated_stakes { get; set; }
    }

    public partial class PollVotePubDTO
    {
        /// <summary>The pool's ticker name.</summary>
        public string? ticker_name { get; set; }

        /// <summary>The pool's name.</summary>
        public string? pool_name { get; set; }

        /// <summary>The Bech32 encoding of the pool hash for the pool that cast this vote.</summary>
        public string pool_id { get; set; }

        /// <summary>The HEX encoding of the transaction carrying this vote.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The vote response string.</summary>
        public string response { get; set; }

        /// <summary>The vote's JSON payload.</summary>
        public string response_json { get; set; }

        /// <summary>The HEX encoding of the extra signer hash attached to the vote transaction.</summary>
        public string extra_sign_hash { get; set; }

        /// <summary>The hexadecimal encoding of the pool verification key hash for the pool that cast this vote.</summary>
        public string cold_vkey { get; set; }

        /// <summary>The number of delegators currently delegating to this pool.</summary>
        public int delegator_count { get; set; }

        /// <summary>The current delegated stakes for this pool (in Lovelace).</summary>
        public ulong delegated_stakes { get; set; }
    }

    public partial class PollSummaryDTO
    {
        /// <summary>The poll's summary by votes count.</summary>
        public PollSummaryVotesDTO votes { get; set; }

        /// <summary>The poll's summary by stakes size.</summary>
        public PollSummaryStakesDTO stakes { get; set; }

        /// <summary>The poll's summary by number of delegators.</summary>
        public PollSummaryDelegatorsDTO delegators { get; set; }
    }

    public partial class PollSummaryVotesDTO
    {
        /// <summary>Total number of votes.</summary>
        public int total { get; set; }

        /// <summary>Counts of votes per choice.</summary>
        public List<int> counts { get; set; }

        /// <summary>Percentages of votes per choice.</summary>
        public List<float> pcts { get; set; }
    }

    public partial class PollSummaryDelegatorsDTO
    {
        /// <summary>Total voting delegators.</summary>
        public int total { get; set; }

        /// <summary>Sums of voting delegators per choice.</summary>
        public List<int> sums { get; set; }

        /// <summary>Percentages of voting delegators per choice.</summary>
        public List<float> pcts { get; set; }
    }

    public partial class PollSummaryStakesDTO
    {
        /// <summary>Total voting stakes.</summary>
        public ulong total { get; set; }

        /// <summary>Sums of voting stakes per choice.</summary>
        public List<ulong> sums { get; set; }

        /// <summary>Percentages of voting stakes per choice.</summary>
        public List<float> pcts { get; set; }
    }
}