using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("tx_in")]
    [Index("redeemer_id", Name = "idx_tx_in_redeemer_id")]
    [Index("tx_in_id", Name = "idx_tx_in_tx_in_id")]
    [Index("tx_out_id", Name = "idx_tx_in_tx_out_id")]
    public partial class TransactionInput
    {
        /// <summary>The transaction input unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Tx table index of the transaction that contains this transaction input.</summary>
        public long tx_in_id { get; set; }

        /// <summary>The Tx table index of the transaction that contains the referenced transaction output.</summary>
        public long tx_out_id { get; set; }

        /// <summary>The index within the transaction outputs.</summary>
        public short tx_out_index { get; set; }

        /// <summary>The Redeemer table index which is used to validate this input.</summary>
        public long? redeemer_id { get; set; }
    }
}
