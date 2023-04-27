using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_asset_cache")]
    [Index("first_mint_tx_id", Name = "_cbi_ac_idx_first_mint_tx_id")]
    [Index("last_mint_tx_id", Name = "_cbi_ac_idx_last_mint_tx_id")]
    public partial class MultiAssetCache
    {
        /// <summary>The MultiAsset unique identifier.</summary>
        [Key]
        public long asset_id { get; set; }

        /// <summary>The MultiAsset creation time (first minting event time).</summary>
        public DateTime creation_time { get; set; }

        /// <summary>The MultiAsset total circulating supply.</summary>
        public decimal total_supply { get; set; }

        /// <summary>The number of mint events for this MultiAsset.</summary>
        public long mint_cnt { get; set; }

        /// <summary>The number of burn events for this MultiAsset.</summary>
        public long burn_cnt { get; set; }

        /// <summary>The Tx table index for the transaction that contains the first minting event for this MultiAsset.</summary>
        public long first_mint_tx_id { get; set; }

        /// <summary>The hash for the transaction that contains the first minting event for this MultiAsset.</summary>
        public string first_mint_tx_hash { get; set; }

        /// <summary>The metadata keys used in the first mint event for this MultiAsset.</summary>
        public string[] first_mint_keys { get; set; }

        /// <summary>The Tx table index for the transaction that contains the last minting event for this MultiAsset.</summary>
        public long last_mint_tx_id { get; set; }

        /// <summary>The hash for the transaction that contains the last minting event for this MultiAsset.</summary>
        public string last_mint_tx_hash { get; set; }

        /// <summary>The metadata keys used in the last mint event for this MultiAsset.</summary>
        public string[] last_mint_keys { get; set; }
    }
}
