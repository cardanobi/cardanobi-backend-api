using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class TransactionOutputDTO
    {
        /// <summary>The index of this transaction output within the transaction.</summary>
        public long index { get; set; }

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public decimal quantity { get; set; }

        /// <summary>The MultiAsset name.</summary>
        public byte[] name { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        [Column(TypeName = "character varying")]
        public string fingerprint { get; set; } = null!;
    }

    public partial class TransactionDTO
    {
        /// <summary>The transaction unique identifier.</summary>
        public long id { get; set; }

        /// <summary>The slot number.</summary>
        public long? slot_no { get; set; }

        /// <summary>The block number.</summary>
        public int? block_no { get; set; }

        /// <summary>The block time (UTCTime).</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime block_time { get; set; }

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

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public List<TransactionOutputDTO> outputs { get; set; }
    }
}
