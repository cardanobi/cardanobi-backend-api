using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_metadata_ref")]
    [Index("registered_tx_id", Name = "idx_pool_metadata_ref_registered_tx_id")]
    [Index("pool_id", "url", "hash", Name = "unique_pool_metadata_ref", IsUnique = true)]
    public partial class PoolMetadata
    {
        /// <summary>The pool metadata ref unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolHash table index of the pool for this reference.</summary>
        public long pool_id { get; set; }

        /// <summary>The URL for the location of the off-chain data.</summary>
        [Column(TypeName = "character varying")]
        public string url { get; set; } = null!;

        /// <summary>The expected hash for the off-chain data.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The Tx table index of the transaction in which provided this metadata reference.</summary>
        public long registered_tx_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the expected hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }

    }
}
