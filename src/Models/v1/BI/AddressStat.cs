﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("_cbi_address_stats_cache")]
    public partial class AddressStat
    {
        /// <summary>The epoch number.</summary>
        [Key]
        public int? epoch_no { get; set; }

        /// <summary>The enterprise or payment address.</summary>
        [Key]
        [Column(TypeName = "character varying")]
        public string? address { get; set; }

        /// <summary>The stake addres unique identifier.</summary>}
        [Key]
        public long? stake_address_id { get; set; }

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }
    }
}
