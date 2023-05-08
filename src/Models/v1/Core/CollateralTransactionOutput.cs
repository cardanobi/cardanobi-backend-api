using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("collateral_tx_out")]
    [Index("inline_datum_id", Name = "collateral_tx_out_inline_datum_id_idx")]
    [Index("reference_script_id", Name = "collateral_tx_out_reference_script_id_idx")]
    [Index("stake_address_id", Name = "collateral_tx_out_stake_address_id_idx")]
    public partial class CollateralTransactionOutput
    {
        /// <summary>The transaction collateral output unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Tx table index of the transaction that contains this transaction output.</summary>
        public long tx_id { get; set; }

        /// <summary>The index of this transaction output with the transaction.</summary>
        public short index { get; set; }

        /// <summary>The human readable encoding of the output address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The raw binary address.</summary>
        public byte[] address_raw { get; set; } = null!;

        /// <summary>Flag which shows if this address is locked by a script.</summary>
        public bool address_has_script { get; set; }

        /// <summary>The payment credential part of the Shelley address. (NULL for Byron addresses). For a script-locked address, this is the script hash.</summary>
        public byte[]? payment_cred { get; set; }

        /// <summary>The StakeAddress table index for the stake address part of the Shelley address. (NULL for Byron addresses).</summary>
        public long? stake_address_id { get; set; }

        /// <summary>The output value (in Lovelace) of the transaction output.</summary>
        [Precision(20, 0)]
        public ulong value { get; set; }

        /// <summary>The hash of the transaction output datum. (NULL for Txs without scripts).</summary>
        public byte[]? data_hash { get; set; }

        /// <summary>This is a description of the multiassets in collateral output. Since the output is not really created, we don't need to add them in separate tables.</summary>
        [Column(TypeName = "character varying")]
        public string multi_assets_descr { get; set; } = null!;

        /// <summary>The inline datum of the output, if it has one. New in v13.</summary>
        public long? inline_datum_id { get; set; }

        /// <summary>The reference script of the output, if it has one. New in v13.</summary>
        public long? reference_script_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the hash of the transaction output datum.</summary>
        public string data_hash_hex { get { return Convert.ToHexString(data_hash).ToLower(); } set { } }
    
    }
}
