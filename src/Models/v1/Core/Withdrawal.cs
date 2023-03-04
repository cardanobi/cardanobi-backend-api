using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("withdrawal")]
    [Index("addr_id", Name = "idx_withdrawal_addr_id")]
    [Index("redeemer_id", Name = "idx_withdrawal_redeemer_id")]
    [Index("tx_id", Name = "idx_withdrawal_tx_id")]
    public partial class Withdrawal
    {
        /// <summary>The withdrawal unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address for which the withdrawal is for.</summary>
        public long addr_id { get; set; }

        /// <summary>The withdrawal amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }

        /// <summary>The Redeemer table index that is related with this withdrawal.</summary>
        public long? redeemer_id { get; set; }

        /// <summary>The Tx table index for the transaction that contains this withdrawal.</summary>
        public long tx_id { get; set; }
    }
}
