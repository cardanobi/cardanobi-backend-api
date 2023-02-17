using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("tx")]
    [Index("block_id", Name = "idx_tx_block_id")]
    [Index("hash", Name = "unique_tx", IsUnique = true)]
    public partial class Transaction
    {
        /// <summary>The transaction unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The hash identifier of the transaction.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The Block table index of the block that contains this transaction.</summary>
        public long block_id { get; set; }

        /// <summary>The index of this transaction with the block (zero based).</summary>
        public int block_index { get; set; }

        /// <summary>The sum of the transaction outputs (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal out_sum { get; set; }

        /// <summary>The fees paid for this transaction.</summary>
        [Precision(20, 0)]
        public decimal fee { get; set; }

        /// <summary>Deposit (or deposit refund) in this transaction. Deposits are positive, refunds negative.</summary>
        public long deposit { get; set; }

        /// <summary>The size of the transaction in bytes.</summary>
        public int size { get; set; }

        /// <summary>Transaction in invalid before this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_before { get; set; }

        /// <summary>Transaction in invalid at or after this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_hereafter { get; set; }

        /// <summary>False if the contract is invalid. True if the contract is valid or there is no contract.</summary>
        public bool valid_contract { get; set; }

        /// <summary>The sum of the script sizes (in bytes) of scripts in the transaction.</summary>
        public int script_size { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }
    }
}
