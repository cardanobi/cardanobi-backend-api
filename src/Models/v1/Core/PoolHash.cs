using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_hash")]
    [Index("hash_raw", Name = "unique_pool_hash", IsUnique = true)]
    public partial class PoolHash
    {
        /// <summary>The pool hash unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The raw bytes of the pool hash.</summary>
        public byte[] hash_raw { get; set; } = null!;

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        [Column(TypeName = "character varying")]
        public string view { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the pool hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash_raw).ToLower(); } set { } }

    }
}
