using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore;

[Table("_cbi_active_stake_cache_epoch")]
public partial class _cbi_active_stake_cache_epoch
{
    [Key]
    public long epoch_no { get; set; }

    [Precision(20, 0)]
    public decimal amount { get; set; }
}
