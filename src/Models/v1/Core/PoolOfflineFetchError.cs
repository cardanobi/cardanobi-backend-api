using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_offline_fetch_error")]
    [Index("pmr_id", Name = "idx_pool_offline_fetch_error_pmr_id")]
    [Index("pool_id", "fetch_time", "retry_count", Name = "unique_pool_offline_fetch_error", IsUnique = true)]
    public partial class PoolOfflineFetchError
    {
        /// <summary>The pool offline fetch error unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolHash table index for the pool this offline fetch error refers.</summary>
        public long pool_id { get; set; }

        /// <summary>The UTC time stamp of the error.</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime fetch_time { get; set; }

        /// <summary>The PoolMetadataRef table index for this offline data.</summary>
        public long pmr_id { get; set; }

        /// <summary>The text of the error.</summary>
        [Column(TypeName = "character varying")]
        public string fetch_error { get; set; } = null!;

        /// <summary>The number of retries.</summary>
        public int retry_count { get; set; }
    }
}
