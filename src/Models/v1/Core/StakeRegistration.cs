using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("stake_registration")]
    [Index("addr_id", Name = "idx_stake_registration_addr_id")]
    [Index("tx_id", Name = "idx_stake_registration_tx_id")]
    public partial class StakeRegistration
    {
        /// <summary>The stake address registration unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address.</summary>
        public long addr_id { get; set; }

        /// <summary>The index of this stake registration within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The epoch in which the registration took place.</summary>
        public int epoch_no { get; set; }

        /// <summary>The Tx table index of the transaction where this stake address was registered.</summary>
        public long tx_id { get; set; }
    }
}
