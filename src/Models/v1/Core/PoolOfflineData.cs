using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_offline_data")]
    [Index("pmr_id", Name = "idx_pool_offline_data_pmr_id")]
    [Index("pool_id", "hash", Name = "unique_pool_offline_data", IsUnique = true)]
    public partial class PoolOfflineData
    {
        /// <summary>The pool offline data unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolHash table index for the pool this offline data refers.</summary>
        public long pool_id { get; set; }

        /// <summary>The pool's ticker name (as many as 5 characters).</summary>
        [Column(TypeName = "character varying")]
        public string ticker_name { get; set; } = null!;

        /// <summary>The hash of the offline data.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The payload as JSON.</summary>
        [Column(TypeName = "jsonb")]
        public string json { get; set; } = null!;

        /// <summary>The raw bytes of the payload.</summary>
        public byte[] bytes { get; set; } = null!;

        /// <summary>The PoolMetadataRef table index for this offline data.</summary>
        public long pmr_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the offline data hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }

    }
}
