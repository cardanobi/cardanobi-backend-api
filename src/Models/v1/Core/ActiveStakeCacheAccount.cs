using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_active_stake_cache_account")]
    public partial class ActiveStakeCacheAccount
    {
        /// <summary>The account unique identifier the given active stake relates to.</summary>
        [Key]
        public long stake_address_id { get; set; }

        /// <summary>The pool unique identifier the given active stake relates to.</summary>
        [Key]
        public long pool_hash_id { get; set; }

        /// <summary>The epoch number the given active stake relates to.</summary>
        [Key]
        public long epoch_no { get; set; }

        /// <summary>The active delegated stake amount for the given account and epoch number.</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }
    }
}
