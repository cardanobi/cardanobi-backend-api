using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("epoch_param")]
    [Index("block_id", Name = "idx_epoch_param_block_id")]
    [Index("cost_model_id", Name = "idx_epoch_param_cost_model_id")]
    public partial class EpochParam
    {
        /// <summary>The epoch param unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The first epoch for which these parameters are valid.</summary>
        public int epoch_no { get; set; }

        /// <summary>The 'a' parameter to calculate the minimum transaction fee.</summary>
        public int min_fee_a { get; set; }

        /// <summary>The 'b' parameter to calculate the minimum transaction fee.</summary>
        public int min_fee_b { get; set; }

        /// <summary>The maximum block size (in bytes).</summary>
        public int max_block_size { get; set; }

        /// <summary>The maximum transaction size (in bytes).</summary>
        public int max_tx_size { get; set; }

        /// <summary>The maximum block header size (in bytes).</summary>
        public int max_bh_size { get; set; }

        /// <summary>The amount (in Lovelace) require for a deposit to register a StakeAddress.</summary>
        [Precision(20, 0)]
        public decimal key_deposit { get; set; }

        /// <summary>The amount (in Lovelace) require for a deposit to register a stake pool.</summary>
        [Precision(20, 0)]
        public decimal pool_deposit { get; set; }

        /// <summary>The maximum number of epochs in the future that a pool retirement is allowed to be scheduled for.</summary>
        public int max_epoch { get; set; }

        /// <summary>The optimal number of stake pools.</summary>
        public int optimal_pool_count { get; set; }

        /// <summary>The influence of the pledge on a stake pool's probability on minting a block.</summary>
        public double influence { get; set; }

        /// <summary>The monetary expansion rate.</summary>
        public double monetary_expand_rate { get; set; }

        /// <summary>The treasury growth rate.</summary>
        public double treasury_growth_rate { get; set; }

        /// <summary>The decentralisation parameter (1 fully centralised, 0 fully decentralised).</summary>
        public double decentralisation { get; set; }

        /// <summary>The protocol major number.</summary>
        public int protocol_major { get; set; }

        /// <summary>The protocol minor number.</summary>
        public int protocol_minor { get; set; }

        /// <summary>The minimum value of a UTxO entry.</summary>
        [Precision(20, 0)]
        public decimal min_utxo_value { get; set; }

        /// <summary>The minimum pool cost.</summary>
        [Precision(20, 0)]
        public decimal min_pool_cost { get; set; }

        /// <summary>The nonce value for this epoch.</summary>
        public byte[]? nonce { get; set; }

        /// <summary>The CostModel table index for the params.</summary>
        public long? cost_model_id { get; set; }

        /// <summary>The per word cost of script memory usage.</summary>
        public double? price_mem { get; set; }

        /// <summary>The cost of script execution step usage.</summary>
        public double? price_step { get; set; }

        /// <summary>The maximum number of execution memory allowed to be used in a single transaction.</summary>
        [Precision(20, 0)]
        public decimal? max_tx_ex_mem { get; set; }

        /// <summary>The maximum number of execution steps allowed to be used in a single transaction.</summary>
        [Precision(20, 0)]
        public decimal? max_tx_ex_steps { get; set; }

        /// <summary>The maximum number of execution memory allowed to be used in a single block.</summary>
        [Precision(20, 0)]
        public decimal? max_block_ex_mem { get; set; }

        /// <summary>The maximum number of execution steps allowed to be used in a single block.</summary>
        [Precision(20, 0)]
        public decimal? max_block_ex_steps { get; set; }

        /// <summary>The maximum Val size.</summary>
        [Precision(20, 0)]
        public decimal? max_val_size { get; set; }

        /// <summary>The percentage of the txfee which must be provided as collateral when including non-native scripts.</summary>
        public int? collateral_percent { get; set; }

        /// <summary>The maximum number of collateral inputs allowed in a transaction.</summary>
        public int? max_collateral_inputs { get; set; }

        /// <summary>The Block table index for the first block where these parameters are valid.</summary>
        public long block_id { get; set; }

        /// <summary>The 32 byte string of extra random-ness to be added into the protocol's entropy pool.</summary>
        public byte[]? extra_entropy { get; set; }

        /// <summary>For Alonzo this is the cost per UTxO word. For Babbage and later per UTxO byte.</summary>
        [Precision(20, 0)]
        public decimal? coins_per_utxo_size { get; set; }

        /// <summary>The nonce value for this epoch in hexadecimal form.</summary>
        public string nonce_hex { get { return Convert.ToHexString(nonce).ToLower(); } set {} }

        // [NotMapped]
        // public string nonce_hex { get { return Convert.ToHexString(nonce).ToLower(); } set { this.nonce_hex = value; } }
    }
}
