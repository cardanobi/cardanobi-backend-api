using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class AddressStatDTO
    {
        /// <summary>The epoch number.</summary>
        public int? epoch_no { get; set; }

        /// <summary>The enterprise or payment address.</summary>
        public string? address { get; set; }

        /// <summary>The stake addres.</summary>}
        public string? stake_address { get; set; } = null!;

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }
    }
}