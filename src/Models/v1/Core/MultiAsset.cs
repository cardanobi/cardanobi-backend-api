using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("multi_asset")]
    [Index("policy", "name", Name = "unique_multi_asset", IsUnique = true)]
    public partial class MultiAsset
    {
        /// <summary>The MultiAsset unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The MultiAsset policy hash.</summary>
        public byte[] policy { get; set; } = null!;

        /// <summary>The MultiAsset name.</summary>
        public byte[] name { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        [Column(TypeName = "character varying")]
        public string fingerprint { get; set; } = null!;

        // Derived fields
        /// <summary>The hexadecimal encoding of the MultiAsset policy hash.</summary>
        public string policy_hex { get { return Convert.ToHexString(policy).ToLower(); } set { } }
 
    }
}
