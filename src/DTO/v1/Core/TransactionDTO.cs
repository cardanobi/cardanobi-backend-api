using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class TransactionAmountDTO 
    {
        /// <summary>The Lovelace or Multi Asset denominated value of this input/output.</summary>
        [Precision(20, 0)]
        public decimal value { get; set; }

        /// <summary>Lovelace or name of the Multi Asset denominating this value.</summary>
        public string unit { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the Multi Asset.</summary>
        [Column(TypeName = "character varying")]
        public string asset_fingerprint { get; set; } = null!;
    }
    public partial class TransactionOutputProjectionDTO
    {
        /// <summary>The index of this transaction output within the transaction.</summary>
        public long output_index { get; set; }

        /// <summary>The output value (in Lovelace) of the transaction output.</summary>
        [Precision(20, 0)]
        public decimal lovelace_value { get; set; }

        /// <summary>The human readable encoding of the output address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; } = null!;

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public decimal asset_quantity { get; set; }

        /// <summary>The MultiAsset name.</summary>
        // public byte[] asset_name { get; set; } = null!;
        public string asset_name { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        [Column(TypeName = "character varying")]
        public string asset_fingerprint { get; set; } = null!;

        /// <summary>Flag which shows if this output is a collateral output.</summary>
        public bool is_collateral { get; set; }

        /// <summary>The hash of the transaction output datum. (NULL for Txs without scripts).</summary>
        public string data_hash { get; set; } = null!;

        /// <summary>The actual datum data in CBOR format.</summary>
        public string inline_datum_cbor { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash of the reference script of the output.</summary>
        public string reference_script_hash { get; set; } = null!;
    }

    public partial class TransactionOutputDTO
    {
        /// <summary>The index of this transaction output within the transaction.</summary>
        public long output_index { get; set; }

        /// <summary>The human readable encoding of the output address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string tx_hash_hex { get; set; } = null!;

        /// <summary>Flag which shows if this output is a collateral output.</summary>
        public bool is_collateral { get; set; }

        /// <summary>The hash of the transaction output datum. (NULL for Txs without scripts).</summary>
        public string data_hash { get; set; } = null!;

        /// <summary>The actual datum data in CBOR format.</summary>
        public string inline_datum_cbor { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash of the reference script of the output.</summary>
        public string reference_script_hash { get; set; } = null!;

        /// <summary>The list of transaction amounts.</summary>
        public List<TransactionAmountDTO> amounts { get; set; } = null!;
    }

    public partial class TransactionInputProjectionDTO
    {
        /// <summary>The index of this transaction output within the transaction.</summary>
        public long output_index { get; set; }

        /// <summary>The output value (in Lovelace) of the transaction output.</summary>
        [Precision(20, 0)]
        public decimal lovelace_value { get; set; }

        /// <summary>The human readable encoding of the input address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; } = null!;

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public decimal asset_quantity { get; set; }

        /// <summary>The MultiAsset name.</summary>
        // public byte[] asset_name { get; set; } = null!;
        public string asset_name { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        [Column(TypeName = "character varying")]
        public string asset_fingerprint { get; set; } = null!;

        /// <summary>Flag which shows if this input is a collateral consumed in case of a script validation failure.</summary>
        public bool is_collateral { get; set; }

        /// <summary>Flag which shows if this input is a reference transaction input.</summary>
        public bool is_reference { get; set; }

        /// <summary>The hash of the transaction output datum. (NULL for Txs without scripts).</summary>
        public string data_hash { get; set; } = null!;

        /// <summary>The actual datum data in CBOR format.</summary>
        public string inline_datum_cbor { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash of the reference script of the input.</summary>
        public string reference_script_hash { get; set; } = null!;

    }

    public partial class TransactionInputDTO
    {
        /// <summary>The index of this transaction output within the transaction.</summary>
        public long output_index { get; set; }

        /// <summary>The human readable encoding of the input address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string tx_hash_hex { get; set; } = null!;

        /// <summary>Flag which shows if this input is a collateral consumed in case of a script validation failure.</summary>
        public bool is_collateral { get; set; }

        /// <summary>Flag which shows if this input is a reference transaction input.</summary>
        public bool is_reference { get; set; }

        /// <summary>The hash of the transaction output datum. (NULL for Txs without scripts).</summary>
        public string data_hash { get; set; } = null!;

        /// <summary>The actual datum data in CBOR format.</summary>
        public string inline_datum_cbor { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash of the reference script of the input.</summary>
        public string reference_script_hash { get; set; } = null!;

        /// <summary>The list of transaction amounts.</summary>
        public List<TransactionAmountDTO> amounts { get; set; } = null!;
    }

    public partial class TransactionDTO
    {
        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string tx_hash_hex { get; set; } = null!;

        /// <summary>The transaction unique identifier.</summary>
        public long id { get; set; }

        /// <summary>The block number.</summary>
        public int? block_no { get; set; }

        /// <summary>The slot number.</summary>
        public long? slot_no { get; set; }

        /// <summary>The block time (UTCTime).</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime block_time { get; set; }

        /// <summary>The index of this transaction with the block (zero based).</summary>
        public int block_index { get; set; }

        /// <summary>The sum of the transaction outputs (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal out_sum { get; set; }

        /// <summary>The fees paid for this transaction.</summary>
        [Precision(20, 0)]
        public decimal fee { get; set; }

        /// <summary>Deposit (or deposit refund) in this transaction. Deposits are positive, refunds negative.</summary>
        public long deposit { get; set; }

        /// <summary>The size of the transaction in bytes.</summary>
        public int size { get; set; }

        /// <summary>Transaction in invalid before this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_before { get; set; }

        /// <summary>Transaction in invalid at or after this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_hereafter { get; set; }

        /// <summary>False if the contract is invalid. True if the contract is valid or there is no contract.</summary>
        public bool valid_contract { get; set; }

        /// <summary>The sum of the script sizes (in bytes) of scripts in the transaction.</summary>
        public int script_size { get; set; }

        /// <summary>The list of transaction output amounts.</summary>
        public List<TransactionAmountDTO> output_amounts { get; set; } = null!;

        /// <summary>The count of withdrawals from a reward account in this transaction.</summary>
        public int withdrawalCount { get; set; }

        /// <summary>The count of Multi-Asset mint events in this transaction.</summary>
        public int assetMintCount { get; set; }

        /// <summary>The count of metadata attached to this transaction.</summary>
        public int metadataCount { get; set; }

        /// <summary>The count of stake address registration in this transaction.</summary>
        public int stakeRegistrationCount { get; set; }

        /// <summary>The count of stake address deregistration in this transaction.</summary>
        public int stakeDeregistrationCount { get; set; }

        /// <summary>The count of delegation from a stake address to a stake pool in this transaction.</summary>
        public int delegationCount { get; set; }

        /// <summary>The count of payments from the treasury to a stake address in this transaction.</summary>
        public int treasuryCount { get; set; }

        /// <summary>The count of payments from the reserves to a stake address in this transaction.</summary>
        public int reserveCount { get; set; }

        /// <summary>The count of transfers between the reserves pot and the treasury pot in this transaction.</summary>
        public int potTransferCount { get; set; }

        /// <summary>The count of Cardano parameter change proposals in this transaction.</summary>
        public int paramProposalCount { get; set; }

        /// <summary>The count of pool retirement notifications in this transaction.</summary>
        public int poolRetireCount { get; set; }

        /// <summary>The count of on-chain pool updates in this transaction.</summary>
        public int poolUpdateCount { get; set; }
    }

    public partial class TransactionUtxoDTO
    {
        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The transaction unique identifier.</summary>
        public long id { get; set; }

        /// <summary>The slot number.</summary>
        public long? slot_no { get; set; }

        /// <summary>The block number.</summary>
        public int? block_no { get; set; }

        /// <summary>The block time (UTCTime).</summary>
        [Column(TypeName = "timestamp without time zone")]
        public DateTime block_time { get; set; }

        /// <summary>The index of this transaction with the block (zero based).</summary>
        public int block_index { get; set; }

        /// <summary>The sum of the transaction outputs (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal out_sum { get; set; }

        /// <summary>The fees paid for this transaction.</summary>
        [Precision(20, 0)]
        public decimal fee { get; set; }

        /// <summary>Deposit (or deposit refund) in this transaction. Deposits are positive, refunds negative.</summary>
        public long deposit { get; set; }

        /// <summary>The size of the transaction in bytes.</summary>
        public int size { get; set; }

        /// <summary>Transaction in invalid before this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_before { get; set; }

        /// <summary>Transaction in invalid at or after this slot number.</summary>
        [Precision(20, 0)]
        public decimal? invalid_hereafter { get; set; }

        /// <summary>False if the contract is invalid. True if the contract is valid or there is no contract.</summary>
        public bool valid_contract { get; set; }

        /// <summary>The sum of the script sizes (in bytes) of scripts in the transaction.</summary>
        public int script_size { get; set; }

        /// <summary>The list of transaction outputs.</summary>
        public List<TransactionOutputDTO> outputs { get; set; }

        /// <summary>The list of transaction inputs.</summary>
        public List<TransactionInputDTO> inputs { get; set; }
    }
}
