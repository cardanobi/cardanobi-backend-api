using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_retire")]
    [Index("announced_tx_id", Name = "idx_pool_retire_announced_tx_id")]
    [Index("hash_id", Name = "idx_pool_retire_hash_id")]
    public partial class PoolRetire
    {
        /// <summary>The pool retire entry unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolHash table index of the pool this retirement refers to.</summary>
        public long hash_id { get; set; }

        /// <summary>The index of this pool retirement within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The Tx table index of the transaction where this pool retirement was announced.</summary>
        public long announced_tx_id { get; set; }

        /// <summary>The epoch where this pool retires.</summary>
        public int retiring_epoch { get; set; }
    }
}
