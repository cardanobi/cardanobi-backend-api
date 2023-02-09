using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("pool_stat_view")]
    // [PrimaryKey(nameof(State), nameof(LicensePlate))]
    public partial class PoolStat
    {
        /// <summary>The epoch number.</summary>
        [Key]
        public int? epoch_no { get; set; }

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        [Key]
        [Column(TypeName = "character varying")]
        public string? pool_hash { get; set; }

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }
    }
}
