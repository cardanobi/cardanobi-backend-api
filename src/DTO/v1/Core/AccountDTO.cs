using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class AccountInfoDTO
    {
        /// <summary>The Bech32 encoded version of the account's stake address</summary>
        public string stake_address { get; set; } = null!;

        /// <summary>Boolean flag indicating if the account is registered (true) or deregistered (false) on-chain.</summary>
        public bool? is_registered { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the last registration/deregistration transaction for this account.</summary>
        public string? last_reg_dereg_tx { get; set; }

        /// <summary>Epoch number when the account was last registered/deregistered.</summary>
        public decimal? last_reg_dereg_epoch_no { get; set; }

        /// <summary>The Bech32 encoding of the pool hash this account is delegated to.</summary>
        public string? pool_id { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the last delegation transaction for this account.</summary>
        public string? last_deleg_tx { get; set; }

        /// <summary>Epoch number when the current delegation became active for this account.</summary>
        public decimal? delegated_since_epoch_no { get; set; }

        /// <summary>The total ADA balance of this account, e.g. controlled stakes + available rewards.</summary>
        public decimal? total_balance { get; set; }

        /// <summary>The total ADA stakes controlled by this account.</summary>
        public decimal? controlled_stakes { get; set; }

        /// <summary>The total historical ADA rewards earned by this account.</summary>
        public decimal? total_rewards { get; set; }

        /// <summary>The total historical ADA rewards withdrew from this account.</summary>
        public decimal? total_withdrawals { get; set; }

        /// <summary>The available ADA rewards for this account.</summary>
        public decimal? available_rewards { get; set; }
    }

    public partial class AccountRewardDTO
    {
        /// <summary>The epoch in which the reward was earned. For pool and leader rewards spendable in epoch N, this will be N - 2, for treasury and reserves N - 1 and for refund N.</summary>
        public long earned_epoch { get; set; }

        /// <summary>The epoch in which the reward is actually distributed and can be spent.</summary>
        public long spendable_epoch { get; set; }

        /// <summary>The source of the rewards; pool member, pool leader, treasury or reserves payment and pool deposits refunds</summary>
        public string type { get; set; }

        /// <summary>The hexadecimal encoding of hash for the pool the stake address was delegated to when the reward is earned or for the pool that there is a deposit refund. Will be NULL for payments from the treasury or the reserves.</summary>
        public string pool_id_hex { get; set; }

        /// <summary>The reward amount (in Lovelace).</summary>
        public ulong  amount { get; set; }
    }
    public partial class AccountStakingDTO
    {
        /// <summary>The epoch number in which the given stake was active.</summary>
        public int epoch_no { get; set; }

        /// <summaryThe amount (in Lovelace) being staked.</summary>
        public ulong  amount { get; set; }

        /// <summary>The Bech32 encoding of the pool being delegated to.</summary>
        public string pool_id { get; set; }
    }

    public partial class AccountDelegationDTO
    {
        /// <summary>The epoch number in which the given delegation was active.</summary>
        public long epoch_no { get; set; }

          /// <summary>The hexadecimal encoding of the hash identifier of the delegation transaction.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The Bech32 encoding of the pool being delegated to.</summary>
        public string pool_id { get; set; }

        /// <summary>The slot number for this delegation.</summary>
        public long slot_no { get; set; }

        /// <summary>The block number for this delegation.</summary>
        public long block_no { get; set; }

        /// <summary>The block time (UTCTime) for this delegation.</summary>
        public DateTime block_time { get; set; }
    }

    public partial class AccountRegistrationDTO
    {
        /// <summary>The epoch number in which the given registration event occured.</summary>
        public long epoch_no { get; set; }

        /// <summary>The block number for this registration event.</summary>
        public long block_no { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the registration transaction.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The state of the given account following this registration event.</summary>
        public string state { get; set; }
    }

    public partial class AccountWithdrawalDTO
    {
        /// <summary>The block number for this withdrawal transaction.</summary>
        public long block_no { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the withdrawal transaction.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The withdrawal amount (in Lovelace).</summary>
        public ulong  amount { get; set; }
    }

    public partial class AccountMIRDTO
    {
        /// <summary>The epoch number in which the given MIR occured.</summary>
        public long epoch_no { get; set; }

        /// <summary>The block number for this MIR.</summary>
        public long block_no { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the MIR transaction.</summary>
        public string tx_hash_hex { get; set; }

        /// <summary>The MIR amount (in Lovelace).</summary>
        public ulong  amount { get; set; }

        /// <summary>The source of the MIR payment (treasury or reserve).</summary>
        public string mir_type { get; set; }
    }

    public partial class AccountAddressDTO
    {
        /// <summary>The human readable encoding of the associated address. Will be Base58 for Byron era addresses and Bech32 for Shelley era.</summary>
        public string address { get; set; }

        /// <summary>Flag which shows if this address is locked by a script.</summary>
        public bool address_has_script { get; set; }
    }

    public partial class AccountAssetDTO
    {
        /// <summary>The hexadecimal encoding of the MultiAsset policy hash.</summary>
        public string policy_hex { get; set; }

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        public string fingerprint { get; set; } = null!;

        /// <summary>The MultiAsset name.</summary>
        public string name { get; set; }

        /// <summary>The balance of the given MultiAsset held by the account.</summary>
        public ulong quantity { get; set; }
    }
}