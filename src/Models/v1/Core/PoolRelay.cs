using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pool_relay")]
    [Index("update_id", Name = "idx_pool_relay_update_id")]
    [Index("update_id", "ipv4", "ipv6", "dns_name", Name = "unique_pool_relay", IsUnique = true)]
    public partial class PoolRelay
    {
        /// <summary>The pool relay unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The PoolUpdate table index this PoolRelay entry refers to.</summary>
        public long update_id { get; set; }

        /// <summary>The IPv4 address of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? ipv4 { get; set; }

        /// <summary>The IPv6 address of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? ipv6 { get; set; }

        /// <summary>The DNS name of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? dns_name { get; set; }

        /// <summary>The DNS service name of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? dns_srv_name { get; set; }

        /// <summary>The port number of relay.</summary>
        public int? port { get; set; }
    }
}
