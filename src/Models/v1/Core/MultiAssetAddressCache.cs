using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_asset_addresses_cache")]
    [Index("asset_id", Name = "_cbi_aac_idx_1")]
    public partial class MultiAssetAddressCache
    {
        /// <summary>The MultiAsset unique identifier.</summary>
        [Key]
        public long asset_id { get; set; }

        /// <summary>The output address holding a balance in the given MultiAsset.</summary>
        [Key]
        [Column(TypeName = "character varying")]

        public string address { get; set; } = null!;

        /// <summary>The balance held at this address of the given MultiAsset.</summary>
        public decimal quantity { get; set; }
    }
}
