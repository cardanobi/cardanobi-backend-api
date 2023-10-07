using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("reward")]
    [Index("addr_id", Name = "idx_reward_addr_id")]
    [Index("earned_epoch", Name = "idx_reward_earned_epoch")]
    [Index("pool_id", Name = "idx_reward_pool_id")]
    public partial class Reward
    {
        /// <summary>The reward unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address that earned the reward.</summary>
        public long addr_id { get; set; }

        /// <summary>The source of the rewards; pool member, pool leader, treasury or reserves payment and pool deposits refunds</summary>
        public string type { get; set; }

        /// <summary>The reward amount (in Lovelace).</summary>
        // [Precision(20, 0)]
        public ulong amount { get; set; }

        /// <summary>The epoch in which the reward was earned. For pool and leader rewards spendable in epoch N, this will be N - 2, for treasury and reserves N - 1 and for refund N.</summary>
        public long earned_epoch { get; set; }

        /// <summary>The epoch in which the reward is actually distributed and can be spent.</summary>
        public long spendable_epoch { get; set; }

        /// <summary>The PoolHash table index for the pool the stake address was delegated to when the reward is earned or for the pool that there is a deposit refund. Will be NULL for payments from the treasury or the reserves.</summary>
        public long? pool_id { get; set; }

        [ForeignKey("addr_id")]
        public virtual StakeAddress StakeAddress { get; set; }

        [ForeignKey("pool_id")]
        public virtual PoolHash PoolHash { get; set; }
    }
}
