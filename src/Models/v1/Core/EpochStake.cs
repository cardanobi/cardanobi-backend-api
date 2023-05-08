using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("epoch_stake")]
    [Index("addr_id", Name = "idx_epoch_stake_addr_id")]
    [Index("epoch_no", Name = "idx_epoch_stake_epoch_no")]
    [Index("pool_id", Name = "idx_epoch_stake_pool_id")]
    [Index("epoch_no", "addr_id", "pool_id", Name = "unique_stake", IsUnique = true)]
    public partial class EpochStake
    {
        /// <summary>The epoch stake unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address for this EpochStake entry.</summary>
        public long addr_id { get; set; }

        /// <summary>The PoolHash table index for the pool this entry is delegated to.</summary>
        public long pool_id { get; set; }

        /// <summary>The amount (in Lovelace) being staked.</summary>
        [Precision(20, 0)]
        public ulong amount { get; set; }

        /// <summary>The epoch number.</summary>
        public int epoch_no { get; set; }
    }
}
