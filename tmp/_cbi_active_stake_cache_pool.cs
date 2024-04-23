using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore;

[PrimaryKey("pool_id", "epoch_no")]
[Table("_cbi_active_stake_cache_pool")]
public partial class _cbi_active_stake_cache_pool
{
    [Key]
    [Column(TypeName = "character varying")]
    public string pool_id { get; set; } = null!;

    [Key]
    public long epoch_no { get; set; }

    [Precision(20, 0)]
    public decimal amount { get; set; }
}
