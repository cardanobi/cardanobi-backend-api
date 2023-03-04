using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("treasury")]
    [Index("addr_id", Name = "idx_treasury_addr_id")]
    [Index("tx_id", Name = "idx_treasury_tx_id")]
    public partial class Treasury
    {
        /// <summary>The Treasury payment unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address for this Treasury entry.</summary>
        public long addr_id { get; set; }

        /// <summary>The index of this payment certificate within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The payment amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }

        /// <summary>The Tx table index for the transaction that contains this payment.</summary>
        public long tx_id { get; set; }
    }
}
