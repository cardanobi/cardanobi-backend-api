using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("script")]
    [Index("tx_id", Name = "idx_script_tx_id")]
    [Index("hash", Name = "unique_script", IsUnique = true)]
    public partial class Script
    {
        /// <summary>The script unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Tx table index for the transaction where this script first became available.</summary>
        public long tx_id { get; set; }

        /// <summary>The Hash of the Script.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The type of the script. This is currenttly either 'timelock' or 'plutus'.</summary>
        public string? type { get; set; }

        /// <summary>JSON representation of the timelock script, null for other script types.</summary>
        [Column(TypeName = "jsonb")]
        public string? json { get; set; }

        /// <summary>CBOR encoded plutus script data, null for other script types.</summary>
        public byte[]? bytes { get; set; }

        /// <summary>The size of the CBOR serialised script, if it is a Plutus script.</summary>
        public int? serialised_size { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the script hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }
  
    }
}
