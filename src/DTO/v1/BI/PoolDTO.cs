using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public class PoolStatDTO
    {
        /// <summary>The epoch number.</summary>
        public int? epoch_no { get; set; }

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        public string? pool_hash { get; set; }

        /// <summary>The transaction count.</summary>
        public long? tx_count { get; set; }

        /// <summary>The block count.</summary>
        public long? block_count { get; set; }

        /// <summary>The delegator count.</summary>
        public long? delegator_count { get; set; }

        /// <summary>The delegated stake for the given epoch and given pool (active stake).</summary>
        public long? delegated_stakes { get; set; }
    }

    public class PoolStatLifetimeDTO
    {
        /// <summary>The Bech32 encoding of the pool hash.</summary>
        public string? pool_hash { get; set; }

        /// <summary>The lifetime transaction count.</summary>
        public long? tx_count_lifetime { get; set; }

        /// <summary>The lifetime block count.</summary>
        public long? block_count_lifetime { get; set; }

        /// <summary>The lifetime delegator count.</summary>
        public long? delegator_count_lifetime { get; set; }

        /// <summary>The lifetime delegated stake for the given pool (lifetime active stake).</summary>
        public decimal? delegated_stakes_lifetime { get; set; }

        /// <summary>The lifetime average delegator count.</summary>
        public double? delegator_count_lifetime_avg { get; set; }

        /// <summary>The lifetime average delegated stake for the given pool (lifetime average active stake).</summary>
        public double? delegated_stakes_lifetime_avg { get; set; }
    }
}