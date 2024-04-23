using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class ActivePoolStakePerEpochDTO 
    {
        /// <summary>The epoch number.</summary>
        public long epoch_no { get; set; }

        /// <summary>The stake addres.</summary>}
        public string stake_address { get; set; } = null!;

        /// <summaryThe active stake amount (in Lovelace).</summary>
        public decimal amount { get; set; }
    }

    public partial class ActivePoolStakePerPoolPerEpochDTO 
    {
        /// <summary>The stake addres.</summary>}
        public string stake_address { get; set; } = null!;

        /// <summaryThe active stake amount (in Lovelace).</summary>
        public decimal amount { get; set; }
    }
}
