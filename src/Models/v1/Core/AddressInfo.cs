using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    // [Keyless]
    [Table("_cbi_address_info_cache")]
    public partial class AddressInfo
    {
        /// <summary>The address.</summary>
        [Key]
        [Column(TypeName = "character varying")]
        public string? address { get; set; }

        /// <summary>The stake addres unique identifier.</summary>}
        public long? stake_address_id { get; set; }

        /// <summary>The stake address.</summary>
        [Column(TypeName = "character varying")]
        public string? stake_address { get; set; }

        /// <summary>The script hash in HEX form in case this address is locked by a script.</summary>
        public string? script_hash { get; set; }
    }
}
