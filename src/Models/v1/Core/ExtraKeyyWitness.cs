using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("extra_key_witness")]
    [Index("tx_id", Name = "idx_extra_key_witness_tx_id")]
    [Index("hash", Name = "unique_witness", IsUnique = true)]
    public partial class ExtraKeyyWitness
    {
        /// <summary>The Extra Key Witness unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The hash of the witness.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The id of the tx this witness belongs to.</summary>
        public long tx_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the witness hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }
    }
}
