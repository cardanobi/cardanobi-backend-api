using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("datum")]
    [Index("tx_id", Name = "idx_datum_tx_id")]
    [Index("hash", Name = "unique_datum", IsUnique = true)]
    public partial class Datum
    {
        /// <summary>The Datum unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Hash of the Datum.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The Tx table index for the transaction where this script first became available.</summary>
        public long tx_id { get; set; }

        /// <summary>The actual data in JSON format (detailed schema).</summary>
        [Column(TypeName = "jsonb")]
        public string? value { get; set; }

        /// <summary>The actual data in CBOR format.</summary>
        public byte[] bytes { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the hash of the Datum.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }
    }
}
