using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApiCore.Models
{
    [Table("slot_leader")]
    [Index("pool_hash_id", Name = "idx_slot_leader_pool_hash_id")]
    [Index("hash", Name = "unique_slot_leader", IsUnique = true)]
    public partial class SlotLeader
    {
        /// <summary>The slot leader unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The hash of the block producer identifier.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>If the slot leader is a pool, an index into the PoolHash table.</summary>
        public long? pool_hash_id { get; set; }

        /// <summary>An auto-generated description of the slot leader.</summary>
        [Column(TypeName = "character varying")]
        public string description { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the block producer hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }
    }
}
