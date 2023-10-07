using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("_cbi_pool_stats_cache")]
    // [PrimaryKey(nameof(State), nameof(LicensePlate))]
    public partial class PoolStat
    {
        /// <summary>The epoch number.</summary>
        [Key]
        public int? epoch_no { get; set; }

        // /// <summary>The Bech32 encoding of the pool hash.</summary>
        // [Key]
        // [Column(TypeName = "character varying")]
        // public string? pool_hash { get; set; }

        /// <summary>The pool hash unique identifier.</summary>
        [Key]
        public long? pool_hash_id { get; set; }

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }

        /// <summary>The block count.</summary>
        public long? block_count { get; set; }

        /// <summary>The delegator count.</summary>
        public long? delegator_count { get; set; }

        /// <summary>The delegated stake for the given epoch and given pool (active stake).</summary>
        public long? delegated_stakes { get; set; }

        [ForeignKey("pool_hash_id")]
        public virtual PoolHash PoolHash { get; set; }
    }
}
