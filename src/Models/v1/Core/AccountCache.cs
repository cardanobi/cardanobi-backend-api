using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("_cbi_stake_distribution_cache")]
    public partial class AccountCache
    {
        // /// <summary>The Bech32 encoded version of the account's stake address</summary>
        // [Key]
        // [Column(TypeName = "character varying")]
        // public string stake_address { get; set; } = null!;

        /// <summary>The account's stake address unique identifier.</summary>
        [Key]
        public long stake_address_id { get; set; }

        /// <summary>Boolean flag indicating if the stake address is registered (true) or deregistered (false) on-chain.</summary>
        public bool? is_registered { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the last registration/deregistration transaction for this stake address.</summary>
        [Column(TypeName = "character varying")]
        public string? last_reg_dereg_tx { get; set; }

        /// <summary>Epoch number when the stake address was last registered/deregistered.</summary>
        public decimal? last_reg_dereg_epoch_no { get; set; }

        // /// <summary>The Bech32 encoding of the pool hash this account is delegated to.</summary>
        // [Column(TypeName = "character varying")]
        // public string? pool_id { get; set; }

        /// <summary>The pool hash unique identifier.</summary>
        public long? pool_hash_id { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the last delegation transaction for this stake address.</summary>
        [Column(TypeName = "character varying")]
        public string? last_deleg_tx { get; set; }

        /// <summary>Epoch number when the current delegation became active for this stake address.</summary>
        public decimal? delegated_since_epoch_no { get; set; }

        /// <summary>The total ADA balance of this account, e.g. controlled stakes + available rewards.</summary>
        public decimal? total_balance { get; set; }

        /// <summary>The total ADA stakes controlled by this account.</summary>
        public decimal? utxo { get; set; }

        /// <summary>The total historical ADA rewards earned by this account.</summary>
        public decimal? rewards { get; set; }

        /// <summary>The total historical ADA rewards withdrew from this account.</summary>
        public decimal? withdrawals { get; set; }

        /// <summary>The available ADA rewards for this account.</summary>
        public decimal? rewards_available { get; set; }


        [ForeignKey("stake_address_id")]
        public virtual StakeAddress StakeAddress { get; set; }

        [ForeignKey("pool_hash_id")]
        public virtual PoolHash PoolHash { get; set; }
    }
}
