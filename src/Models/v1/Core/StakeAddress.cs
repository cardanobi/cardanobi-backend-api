using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("stake_address")]
    [Index("hash_raw", Name = "idx_stake_address_hash_raw")]
    [Index("hash_raw", Name = "unique_stake_address", IsUnique = true)]
    public partial class StakeAddress
    {
        /// <summary>The stake address unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The raw bytes of the stake address hash.</summary>
        public byte[] hash_raw { get; set; } = null!;

        /// <summary>The Bech32 encoded version of the stake address.</summary>
        [Column(TypeName = "character varying")]
        public string view { get; set; } = null!;

        /// <summary>The script hash, in case this address is locked by a script.</summary>
        public byte[]? script_hash { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the stake address hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash_raw).ToLower(); } set { } }

        /// <summary>The hexadecimal encoding of the script hash.</summary>
        public string script_hash_hex { get { return script_hash != null ? Convert.ToHexString(script_hash).ToLower():""; } set { } }
    }
}
