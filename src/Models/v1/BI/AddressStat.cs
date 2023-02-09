using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("address_stat_view")]
    public partial class AddressStat
    {
        /// <summary>The epoch number.</summary>
        [Key]
        public int? epoch_no { get; set; }

        /// <summary>The stake address.</summary>
        [Key]
        [Column(TypeName = "character varying")]
        public string? stake_address { get; set; }

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }
    }
}
