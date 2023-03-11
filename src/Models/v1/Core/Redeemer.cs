using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.Models
{
    [Table("redeemer")]
    [Index("redeemer_data_id", Name = "redeemer_redeemer_data_id_idx")]
    public partial class Redeemer
    {
        /// <summary>The redeemer unique identifier.</summary>
        [Key]
        public long id { get; set; }

        /// <summary>The Tx table index that contains this redeemer.</summary>
        public long tx_id { get; set; }

        /// <summary>The budget in Memory to run a script.</summary>
        public long unit_mem { get; set; }

        /// <summary>The budget in Cpu steps to run a script.</summary>
        public long unit_steps { get; set; }

        /// <summary>The budget in fees to run a script. The fees depend on the ExUnits and the current prices. Is null when --disable-ledger is enabled. New in v13: became nullable.</summary>
        [Precision(20, 0)]
        public decimal? fee { get; set; }

        /// <summary>What kind of validation this redeemer is used for. It can be one of 'spend', 'mint', 'cert', 'reward'.</summary>
        public string purpose { get; set; }

        /// <summary>The index of the redeemer pointer in the transaction.</summary>
        public int index { get; set; }

        /// <summary>The script hash this redeemer is used for.</summary>
        public byte[]? script_hash { get; set; }

        /// <summary>The data related to this redeemer. New in v13: renamed from datum_id.</summary>
        public long redeemer_data_id { get; set; }

        // Derived fields
        /// <summary>The hexadecimal encoding of the script hash.</summary>
        public string script_hash_hex { get { return Convert.ToHexString(script_hash).ToLower(); } set { } }
    }
}
