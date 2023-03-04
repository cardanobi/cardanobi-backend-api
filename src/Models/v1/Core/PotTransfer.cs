using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("pot_transfer")]
    public partial class PotTransfer
    {
        /// <summary>The transfer unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The index of this transfer certificate within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The amount (in Lovelace) the treasury balance changes by.</summary>
        [Precision(20, 0)]
        public decimal treasury { get; set; }

        /// <summary>The amount (in Lovelace) the reserves balance changes by.</summary>
        [Precision(20, 0)]
        public decimal reserves { get; set; }

        /// <summary>The Tx table index for the transaction that contains this transfer.</summary>
        public long tx_id { get; set; }
    }
}
