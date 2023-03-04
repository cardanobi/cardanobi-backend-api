using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("tx_metadata")]
    [Index("tx_id", Name = "idx_tx_metadata_tx_id")]
    public partial class TransactionMetadata
    {
        /// <summary>The transaction metadata unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The metadata key (a Word64/unsigned 64 bit number).</summary>
        [Precision(20, 0)]
        public decimal key { get; set; }

        /// <summary>The JSON payload if it can be decoded as JSON.</summary>
        [Column(TypeName = "jsonb")]
        public string? json { get; set; }

        /// <summary>The raw bytes of the payload.</summary>
        public byte[] bytes { get; set; } = null!;

        /// <summary>The Tx table index of the transaction where this metadata was included.</summary>
        public long tx_id { get; set; }
    }
}
