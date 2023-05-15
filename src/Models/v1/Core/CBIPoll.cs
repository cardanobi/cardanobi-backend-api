using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_polls")]
    [Index("tx_id", Name = "_cbi_votes_tx_tx_id_key", IsUnique = true)]
    [Index("tx_id", Name = "idx_cbi_votes_tx_tx_id")]
    [Index("question_hash", Name = "_cbi_polls_question_hash_key", IsUnique = true)]
    [Index("question_hash", Name = "idx_cbi_polls_question_hash")]
    public partial class CBIPoll
    {
        /// <summary>The CBI Vote Transaction unique identifier.</summary>
        [Key]
        public int id { get; set; }

        /// <summary>The unique identifier of the transaction carrying this vote.</summary>
        public long tx_id { get; set; }

        /// <summary>The epoch number marking the start of this poll.</summary>
        public int start_epoch_no { get; set; }

        /// <summary>The epoch number marking the end of this poll.</summary>
        public int end_epoch_no { get; set; }

        /// <summary>The poll's question hash.</summary>
        public byte[] question_hash { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string question_hash_hex { get { return Convert.ToHexString(question_hash).ToLower(); } set { } }
    }
}
