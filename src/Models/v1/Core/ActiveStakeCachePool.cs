using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_active_stake_cache_pool")]
    public partial class ActiveStakeCachePool
    {
        /// <summary>The pool unique identifier the given active stake relates to.</summary>
        [Key]
        [Column(TypeName = "character varying")]
        public string pool_id { get; set; } = null!;

        /// <summary>The epoch number the given active stake relates to.</summary>
        [Key]
        public long epoch_no { get; set; }

        /// <summary>The active delegated stake amount for the given pool identifier and epoch number.</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }
    }
}
