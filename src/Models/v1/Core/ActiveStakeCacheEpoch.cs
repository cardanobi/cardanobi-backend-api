using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_active_stake_cache_epoch")]
    public partial class ActiveStakeCacheEpoch
    {
        /// <summary>The epoch number the given active stake is given for.</summary>
        [Key]
        public long epoch_no { get; set; }

        /// <summary>The active delegated stake amount for the given epoch number.</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }
    }
}
