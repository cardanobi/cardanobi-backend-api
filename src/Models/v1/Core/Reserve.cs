using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("reserve")]
    [Index("addr_id", Name = "idx_reserve_addr_id")]
    [Index("tx_id", Name = "idx_reserve_tx_id")]
    public partial class Reserve
    {
        /// <summary>The reserve transaction unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address for this Treasury entry.</summary>
        public long addr_id { get; set; }

        /// <summary>The index of this payment certificate within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The payment amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public ulong amount { get; set; }

        /// <summary>The Tx table index for the transaction that contains this payment.</summary>
        public long tx_id { get; set; }

        [ForeignKey("addr_id")]
        public virtual StakeAddress StakeAddress { get; set; }
        
        [ForeignKey("tx_id")]
        public virtual Transaction Transaction { get; set; }
    }
}
