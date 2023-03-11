using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_update")]
    [Index("active_epoch_no", Name = "idx_pool_update_active_epoch_no")]
    [Index("hash_id", Name = "idx_pool_update_hash_id")]
    [Index("meta_id", Name = "idx_pool_update_meta_id")]
    [Index("registered_tx_id", Name = "idx_pool_update_registered_tx_id")]
    [Index("reward_addr_id", Name = "idx_pool_update_reward_addr")]
    [Index("registered_tx_id", "cert_index", Name = "unique_pool_update", IsUnique = true)]
    public partial class PoolUpdate
    {
        /// <summary>The pool update unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolHash table index of the pool this update refers to.</summary>
        public long hash_id { get; set; }

        /// <summary>The index of this pool update within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The hash of the pool's VRF key.</summary>
        public byte[] vrf_key_hash { get; set; } = null!;

        /// <summary>The amount (in Lovelace) the pool owner pledges to the pool.</summary>
        [Precision(20, 0)]
        public decimal pledge { get; set; }

        /// <summary>The epoch number where this update becomes active.</summary>
        public long active_epoch_no { get; set; }

        /// <summary>The PoolMetadataRef table index this pool update refers to.</summary>
        public long? meta_id { get; set; }

        /// <summary>The margin (as a percentage) this pool charges.</summary>
        public double margin { get; set; }

        /// <summary>The fixed per epoch fee (in ADA) this pool charges.</summary>
        [Precision(20, 0)]
        public decimal fixed_cost { get; set; }

        /// <summary>The Tx table index of the transaction in which provided this pool update.</summary>
        public long registered_tx_id { get; set; }

        /// <summary>The StakeAddress table index of this pool's rewards address. New in v13: Replaced reward_addr.</summary>
        public long reward_addr_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the VRF key hash.</summary>
        public string vrf_key_hash_hex { get { return Convert.ToHexString(vrf_key_hash).ToLower(); } set { } }
    }
}
