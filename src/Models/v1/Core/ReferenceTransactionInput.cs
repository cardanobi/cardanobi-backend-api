using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("reference_tx_in")]
    [Index("tx_out_id", Name = "reference_tx_in_tx_out_id_idx")]
    public partial class ReferenceTransactionInput
    {
        /// <summary>The reference transaction input unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Tx table index of the transaction that contains this transaction input</summary>
        public long tx_in_id { get; set; }

        /// <summary>The Tx table index of the transaction that contains the referenced output.</summary>
        public long tx_out_id { get; set; }

        /// <summary>The index within the transaction outputs.</summary>
        public short tx_out_index { get; set; }
    }
}
