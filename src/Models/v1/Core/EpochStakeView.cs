using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("epoch_stake_view")]
    public partial class EpochStakeView
    {
        /// <summary>The epoch stake unique identifier.</summary>
        [Key]
        public long? epoch_stake_id { get; set; }

        /// <summary>The amount (in Lovelace) being staked.</summary>
        [Precision(20, 0)]
        public decimal? epoch_stake_amount { get; set; }

        /// <summary>The epoch number.</summary>
        public int? epoch_stake_epoch_no { get; set; }

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        [Column(TypeName = "character varying")]
        public string? pool_hash { get; set; }

        /// <summary>The Bech32 encoded version of the stake address hash.</summary>
        [Column(TypeName = "character varying")]
        public string? stake_address { get; set; }

        /// <summary>The hexadecimal encoding of the script hash, in case this address is locked by a script.</summary>
        public string? stake_address_script_hash_hex { get; set; }

        /// <summary>The stake address unique identifier.</summary>
        public long? stake_address_id { get; set; }
    }
}
