using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("redeemer_data")]
    [Index("tx_id", Name = "redeemer_data_tx_id_idx")]
    [Index("hash", Name = "unique_redeemer_data", IsUnique = true)]
    public partial class RedeemerData
    {
        /// <summary>The redeemer data unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Hash of the Plutus Data.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The Tx table index for the transaction where this script first became available.</summary>
        public long tx_id { get; set; }

        /// <summary>The actual data in JSON format (detailed schema)</summary>
        [Column(TypeName = "jsonb")]
        public string? value { get; set; }

        /// <summary>The actual data in CBOR format</summary>
        public byte[] bytes { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the Plutus Data hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }

    }
}
