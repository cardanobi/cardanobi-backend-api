using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("ma_tx_out")]
    [Index("tx_out_id", Name = "idx_ma_tx_out_tx_out_id")]
    public partial class MultiAssetTransactionOutput
    {
        /// <summary>The MultiAsset transaction unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public ulong quantity { get; set; }

        /// <summary>The TxOut table index for the transaction that this Multi Asset transaction output.</summary>
        public long tx_out_id { get; set; }

        /// <summary>The MultiAsset table index specifying the asset.</summary>
        public long ident { get; set; }
    }
}
