using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_owner")]
    [Index("pool_update_id", Name = "pool_owner_pool_update_id_idx")]
    public partial class PoolOwner
    {
        /// <summary>The pool owner table unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the pool owner's stake address.</summary>
        public long addr_id { get; set; }

        /// <summary>The PoolUpdate table index for the pool. New in v13.</summary>
        public long pool_update_id { get; set; }
    }
}
