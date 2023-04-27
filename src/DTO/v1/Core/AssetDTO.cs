using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiCore.DTO
{
    public partial class AssetListDTO 
    {
        /// <summary>The MultiAsset unique identifier.</summary>
        public long asset_id { get; set; }

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        public string fingerprint { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the MultiAsset policy hash.</summary>
        public string policy_hex { get; set; } = null!;

        /// <summary>The total supply of the Multi Asset.</summary>
        [Precision(20, 0)]
        public decimal total_supply { get; set; }
    }

    public partial class AssetDetailsDTO 
    {
        /// <summary>The MultiAsset unique identifier.</summary>
        public long asset_id { get; set; }

        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        public string fingerprint { get; set; } = null!;

        /// <summary>The hexadecimal encoding of the MultiAsset policy hash.</summary>
        public string policy_hex { get; set; } = null!;

        /// <summary>The MultiAsset name.</summary>
        public string name { get; set; } = null!;

        /// <summary>The MultiAsset creation time (first minting event time).</summary>
        public DateTime creation_time { get; set; }

        /// <summary>The MultiAsset total circulating supply.</summary>
        public decimal total_supply { get; set; }

        /// <summary>The number of mint events for this MultiAsset.</summary>
        public long mint_cnt { get; set; }

        /// <summary>The number of burn events for this MultiAsset.</summary>
        public long burn_cnt { get; set; }

        /// <summary>The hash for the transaction that contains the first minting event for this MultiAsset.</summary>
        public string first_mint_tx_hash { get; set; }

        /// <summary>The metadata keys used in the first mint event for this MultiAsset.</summary>
        public string[] first_mint_keys { get; set; }

        /// <summary>The hash for the transaction that contains the last minting event for this MultiAsset.</summary>
        public string last_mint_tx_hash { get; set; }

        /// <summary>The metadata keys used in the last mint event for this MultiAsset.</summary>
        public string[] last_mint_keys { get; set; }

        /// <summary>The JSON payload of the first mint event for this MultiAsset.</summary>
        public string? first_mint_metadata { get; set; }
    }

    public partial class AssetHistoryDTO 
    {
        /// <summary>The Multi-Asset minting/buring event unique identifier.</summary>
        public long event_id { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction containing this event.</summary>
        public string tx_hash_hex { get; set; }
    
        /// <summary>The amount of the Multi Asset to mint (can be negative to "burn" assets).</summary>
        public decimal quantity { get; set; }

        /// <summary>The event creation time (time of the block containing it).</summary>
        public DateTime event_time { get; set; }

        /// <summary>The block number containing the minting/buring transaction for this event.</summary>
        public int block_no { get; set; }
    }

    public partial class AssetTransactionDTO 
    {
        /// <summary>The transaction unique identifier.</summary>
        public long tx_id { get; set; }

        /// <summary>The hexadecimal encoding of the hash identifier of the transaction.</summary>
        public string hash_hex { get; set; }

        /// <summary>The epoch number.</summary>
        public int epoch_no { get; set; }
        
        /// <summary>The block number containing the minting/buring transaction for this event.</summary>
        public int block_no { get; set; }

        /// <summary>The time (UTCTime) of the block containing this transaction.</summary>
        public DateTime event_time { get; set; }
    }

    public partial class AssetAddressDTO 
    {
        /// <summary>The output address holding a balance in the given Multi-Asset.</summary>
        public string address { get; set; }

        /// <summary>The balance held at this address of the given MultiAsset.</summary>
        // [Precision(20, 0)]
        public decimal quantity { get; set; }
    }

    public partial class AssetPolicyDTO 
    {
        /// <summary>The CIP14 fingerprint for the MultiAsset.</summary>
        public string fingerprint { get; set; }

        /// <summary>The total supply of the Multi Asset.</summary>
        public decimal total_supply { get; set; }
    }
}
