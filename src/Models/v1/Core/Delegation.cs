using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("delegation")]
    [Index("active_epoch_no", Name = "idx_delegation_active_epoch_no")]
    [Index("addr_id", Name = "idx_delegation_addr_id")]
    [Index("pool_hash_id", Name = "idx_delegation_pool_hash_id")]
    [Index("redeemer_id", Name = "idx_delegation_redeemer_id")]
    [Index("tx_id", Name = "idx_delegation_tx_id")]
    public partial class Delegation
    {
        /// <summary>The delegation unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address.</summary>
        public long addr_id { get; set; }

        /// <summary>The index of this delegation within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The PoolHash table index for the pool being delegated to.</summary>
        public long pool_hash_id { get; set; }

        /// <summary>The epoch number where this delegation becomes active.</summary>
        public long active_epoch_no { get; set; }

        /// <summary>The Tx table index of the transaction that contained this delegation.</summary>
        public long tx_id { get; set; }

        /// <summary>The slot number of the block that contained this delegation.</summary>
        public long slot_no { get; set; }

        /// <summary>The Redeemer table index that is related with this certificate.</summary>
        public long? redeemer_id { get; set; }
    }
}
