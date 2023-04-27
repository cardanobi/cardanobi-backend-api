using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ApiCore.Models
{
    [Table("block")]
    [Index("block_no", Name = "idx_block_block_no")]
    [Index("epoch_no", Name = "idx_block_epoch_no")]
    [Index("previous_id", Name = "idx_block_previous_id")]
    [Index("slot_leader_id", Name = "idx_block_slot_leader_id")]
    [Index("slot_no", Name = "idx_block_slot_no")]
    [Index("time", Name = "idx_block_time")]
    [Index("hash", Name = "unique_block", IsUnique = true)]
    public partial class Block
    {
        /// <summary>The block unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The hash identifier of the block.</summary>
        public byte[] hash { get; set; } = null!;

        /// <summary>The epoch number.</summary>
        public int epoch_no { get; set; }

        /// <summary>The slot number.</summary>
        public long? slot_no { get; set; }

        /// <summary>The slot number within an epoch (resets to zero at the start of each epoch).</summary>
        public int? epoch_slot_no { get; set; }

        /// <summary>The block number.</summary>
        public int block_no { get; set; }

        /// <summary>The Block table index of the previous block.</summary>
        public long? previous_id { get; set; }

        /// <summary>The SlotLeader table index of the creator of this block.</summary>
        public long slot_leader_id { get; set; }

        /// <summary>The block size (in bytes). Note, this size value is not expected to be the same as the sum of the tx sizes due to the fact that txs being stored in segwit format and oddities in the CBOR encoding.</summary>
        public int size { get; set; }

        /// <summary>The block time (UTCTime).</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime time { get; set; }

        /// <summary>The number of transactions in this block.</summary>
        public long tx_count { get; set; }

        /// <summary>The block's major protocol number.</summary>
        public int proto_major { get; set; }

        /// <summary>The block's major protocol number.</summary>
        public int proto_minor { get; set; }

        /// <summary>The VRF key of the creator of this block.</summary>
        [Column(TypeName = "character varying")]
        public string? vrf_key { get; set; }

        /// <summary>The hash of the operational certificate of the block producer.</summary>
        public byte[]? op_cert { get; set; }

        /// <summary>The value of the counter used to produce the operational certificate.</summary>
        public long? op_cert_counter { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the block hash.</summary>
        public string hash_hex { get { return Convert.ToHexString(hash).ToLower(); } set { } }

        /// <summary>The hexadecimal encoding of the block producer operational certificate's hash.</summary>
        public string op_cert_hex { get { return op_cert != null ? Convert.ToHexString(op_cert).ToLower():""; } 
                                    set { } }
    }
}
