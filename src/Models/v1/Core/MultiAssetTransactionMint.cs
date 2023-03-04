using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("ma_tx_mint")]
    [Index("tx_id", Name = "idx_ma_tx_mint_tx_id")]
    public partial class MultiAssetTransactionMint
    {
        /// <summary>The Multi-Asset mint event unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The amount of the Multi Asset to mint (can be negative to "burn" assets).</summary>
        [Precision(20, 0)]
        public decimal quantity { get; set; }

        /// <summary>The Tx table index for the transaction that contains this minting event.</summary>
        public long tx_id { get; set; }

        /// <summary>The MultiAsset table index specifying the asset.</summary>
        public long ident { get; set; }
    }
}
