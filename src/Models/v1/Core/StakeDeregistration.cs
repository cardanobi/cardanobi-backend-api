using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("stake_deregistration")]
    [Index("addr_id", Name = "idx_stake_deregistration_addr_id")]
    [Index("redeemer_id", Name = "idx_stake_deregistration_redeemer_id")]
    [Index("tx_id", Name = "idx_stake_deregistration_tx_id")]
    public partial class StakeDeregistration
    {
        /// <summary>The stake address deregistration unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The StakeAddress table index for the stake address.</summary>
        public long addr_id { get; set; }

        /// <summary>The index of this stake deregistration within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The epoch in which the deregistration took place.</summary>
        public int epoch_no { get; set; }

        /// <summary>The Tx table index of the transaction where this stake address was deregistered.</summary>
        public long tx_id { get; set; }

        /// <summary>The Redeemer table index that is related with this certificate.</summary>
        public long? redeemer_id { get; set; }

        [ForeignKey("addr_id")]
        public virtual StakeAddress StakeAddress { get; set; }
        
        [ForeignKey("tx_id")]
        public virtual Transaction Transaction { get; set; }
    }
}
