using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Index("pool_id", Name = "_cbi_pool_params_pool_id_key", IsUnique = true)]
    public partial class CBIPoolParam
    {
        /// <summary>The CBI pool param unique identifier.</summary>
        [Key]
        public int id { get; set; }

        /// <summary>The hexadecimal encoding of the pool hash.</summary>
        [StringLength(64)]
        public string pool_id { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the pool verification key hash.</summary>
        [StringLength(64)]
        public string cold_vkey { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the pool VRF key hash.</summary>
        [StringLength(64)]
        public string vrf_key { get; set; } = null!;
    }
}
