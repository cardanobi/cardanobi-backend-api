using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApiCore.Models
{
    [Table("epoch")]
    [Index("no", Name = "idx_epoch_no")]
    [Index("no", Name = "unique_epoch", IsUnique = true)]
    public partial class Epoch
    {
        /// <summary>The epoch unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The sum of the transaction output values (in Lovelace) in this epoch.</summary>
        [Precision(39, 0)]
        public decimal out_sum { get; set; }

        /// <summary>The sum of the fees (in Lovelace) in this epoch.</summary>
        [Precision(20, 0)]
        public decimal fees { get; set; }

        /// <summary>The number of transactions in this epoch.</summary>
        public int tx_count { get; set; }

        /// <summary>The number of blocks in this epoch.</summary>
        public int blk_count { get; set; }

        /// <summary>The epoch number.</summary>
        public int no { get; set; }

        /// <summary>The epoch start time.</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime start_time { get; set; }

        /// <summary>The epoch end time.</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime end_time { get; set; }
    }
}
