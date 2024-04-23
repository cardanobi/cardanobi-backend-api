using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore;

[PrimaryKey("stake_address_id", "pool_hash_id", "epoch_no")]
[Table("_cbi_active_stake_cache_account")]
[Index("pool_hash_id", "epoch_no", Name = "_cbi_casca_idx_pool_id_epoch_no")]
[Index("stake_address_id", "epoch_no", Name = "_cbi_casca_idx_stake_address_epoch_no")]
public partial class _cbi_active_stake_cache_account
{
    [Key]
    public long stake_address_id { get; set; }

    [Key]
    public long pool_hash_id { get; set; }

    [Key]
    public long epoch_no { get; set; }

    [Precision(20, 0)]
    public decimal? amount { get; set; }
}
