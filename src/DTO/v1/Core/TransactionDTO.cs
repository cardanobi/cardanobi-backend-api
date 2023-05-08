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
        public ulong value { get; set; }

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
        public ulong lovelace_value { get; set; }

        /// <summary>The human readable encoding of the output address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; } = null!;

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public ulong asset_quantity { get; set; }

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
        public ulong lovelace_value { get; set; }

        /// <summary>The human readable encoding of the input address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        [Column(TypeName = "character varying")]
        public string address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; } = null!;

        /// <summary>The Multi Asset transaction output amount (denominated in the Multi Asset).</summary>
        [Precision(20, 0)]
        public ulong asset_quantity { get; set; }

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

        /// <summary>The count of inputs in this transaction.</summary>
        public int inputCount { get; set; }

        /// <summary>The count of outputs in this transaction.</summary>
        public int outputCount { get; set; }

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

        /// <summary>The count of redeemers in this transaction.</summary>
        public int redeemerCount { get; set; }
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

    public partial class TransactionStakeAddressDTO 
    {
        /// <summary>The index of this stake registration within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The epoch in which the registration took place.</summary>
        public int epoch_no { get; set; }

        /// <summary>The Bech32 encoded version of the stake address.</summary>
        [Column(TypeName = "character varying")]
        public string stake_address { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the script hash, in case this address is locked by a script.</summary>
        public string script_hash_hex { get; set; }

        /// <summary>True if the transaction is a registration, False if it is a deregistration.</summary>
        public bool is_registration { get; set; }
    }

    public partial class TransactionStakeAddressDelegationDTO 
    {
        /// <summary>The index of this delegation within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The epoch number where this delegation becomes active.</summary>
        public long active_epoch_no	 { get; set; }

        /// <summary>The Bech32 encoded version of the stake address.</summary>
        [Column(TypeName = "character varying")]
        public string stake_address { get; set; } = null!;

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        [Column(TypeName = "character varying")]
        public string pool_hash_bech32 { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the pool hash.</summary>
        public string pool_hash_hex {  get; set; } = null!;
    }

    public partial class TransactionStakeAddressWithdrawalDTO 
    {
        /// <summary>The Bech32 encoded version of the stake address.</summary>
        public string stake_address { get; set; } = null!;

        /// <summary>The withdrawal amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }

        /// <summary>The Redeemer table index that is related with this withdrawal.</summary>
        public long? redeemer_id { get; set; }
    }

    public partial class TransactionTreasuryDTO 
    {
        /// <summary>The index of this payment certificate within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The Bech32 encoded version of the stake address.</summary>
        public string stake_address { get; set; } = null!;

        /// <summary>The treasury payment amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }
    }

    public partial class TransactionReserveDTO 
    {
        /// <summary>The index of this payment certificate within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The Bech32 encoded version of the stake address.</summary>
        public string stake_address { get; set; } = null!;

        /// <summary>The reserves payment amount (in Lovelace).</summary>
        [Precision(20, 0)]
        public decimal amount { get; set; }
    }

    public partial class TransactionRetiringPoolDTO 
    {
        /// <summary>The index of this pool retirement within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        [Column(TypeName = "character varying")]
        public string pool_hash_bech32 { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the pool hash.</summary>
        public string pool_hash_hex {  get; set; } = null!;

        /// <summary>The epoch where this pool retires.</summary>
        public int retiring_epoch { get; set; }
    }

    public partial class PoolRelayDTO
    {
        /// <summary>The IPv4 address of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? ipv4 { get; set; }

        /// <summary>The IPv6 address of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? ipv6 { get; set; }

        /// <summary>The DNS name of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? dns_name { get; set; }

        /// <summary>The DNS service name of the relay.</summary>
        [Column(TypeName = "character varying")]
        public string? dns_srv_name { get; set; }

        /// <summary>The port number of relay.</summary>
        public int? port { get; set; }
    }
    public partial class PoolOfflineDataDTO
    {
        /// <summary>The pool's ticker name (as many as 5 characters).</summary>
        public string ticker_name { get; set; } = null!;

        /// <summary>The URL for the location of the off-chain data.</summary>
        public string url { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the offline data hash.</summary>
        public string hash_hex { get; set; } = null!;

        /// <summary>The payload as JSON.</summary>
        public string json { get; set; } = null!;

    }
    public partial class TransactionUpdatingPoolDTO 
    {
        /// <summary>The index of this pool update within the certificates of this transaction.</summary>
        public int cert_index { get; set; }

        /// <summary>The Bech32 encoding of the pool hash.</summary>
        public string pool_hash_bech32 { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the pool hash.</summary>
        public string pool_hash_hex { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the VRF key hash.</summary>
        public string vrf_key_hash_hex { get; set; }

        /// <summary>The hexadecimal encoding of the pool reward address hash.</summary>
        public string reward_addr_hash_hex { get; set; } = null!;

        /// <summary>The amount (in Lovelace) the pool owner pledges to the pool.</summary>
        [Precision(20, 0)]
        public decimal pledge { get; set; }

         /// <summary>The margin (as a percentage) this pool charges.</summary>
        public double margin { get; set; }

        /// <summary>The fixed per epoch fee (in ADA) this pool charges.</summary>
        [Precision(20, 0)]
        public decimal fixed_cost { get; set; }

        /// <summary>The epoch number where this update becomes active.</summary>
        public long active_epoch_no { get; set; }

        /// <summary>The list of pool owners stake addresses.</summary>
        public List<string> owners_addresses { get; set; }

        /// <summary>The pool relays updates.</summary>
        public List<PoolRelayDTO> relays { get; set; }

        /// <summary>The pool offline metadata.</summary>
        public PoolOfflineDataDTO offline_data { get; set; }
    }

    public partial class TransactionMetadataDTO
    {
        /// <summary>The metadata key (a Word64/unsigned 64 bit number).</summary>
        [Precision(20, 0)]
        public decimal key { get; set; }

        /// <summary>The JSON payload if it can be decoded as JSON.</summary>
        public string? json { get; set; }
    }

    public partial class MultiAssetTransactionMintDTO
    {
        /// <summary>The amount of the Multi Asset to mint (can be negative to "burn" assets).</summary>
        [Precision(20, 0)]
        public decimal quantity { get; set; }

        /// <summary>The hexadecimal encoding of the MultiAsset policy hash.</summary>
        public string policy_hex { get; set; } = null!;

        /// <summary>The MultiAsset name.</summary>
        public string name { get; set; } = null!;

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        public string fingerprint { get; set; } = null!;
    }

    public partial class RedeemerDTO
    {
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

        /// <summary>The hexadecimal encoding of the script hash this redeemer is used for.</summary>
        public string script_hash_hex { get; set; }

        /// <summary>The hexadecimal encoding of the Plutus Data hash.</summary>
        public string hash_hex { get; set; }

        /// <summary>The actual Plutus data in JSON format (detailed schema)</summary>
        public string? data_json { get; set; }

        /// <summary>The actual Plutus data in CBOR format</summary>
        public string data_cbor { get; set; } = null!;
    }
}
