using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ApiCore.DTO;

namespace ApiCore.Models
{
    public partial class cardanobiCoreContext : DbContext
    {
        // public cardanobiCoreContext()
        // {
        // }

        public cardanobiCoreContext(DbContextOptions<cardanobiCoreContext> options)
            : base(options)
        {
        }

        // Core Domain

        public virtual DbSet<Epoch> Epoch { get; set; } = null!;
        public virtual DbSet<EpochParam> EpochParam { get; set; } = null!;
        public virtual DbSet<EpochStake> EpochStake { get; set; } = null!;
        public virtual DbSet<EpochStakeView> EpochStakeView { get; set; } = null!;
        public virtual DbSet<PoolHash> PoolHash { get; set; } = null!;
        public virtual DbSet<PoolMetadata> PoolMetadata { get; set; } = null!;
        public virtual DbSet<PoolOfflineData> PoolOfflineData { get; set; } = null!;
        public virtual DbSet<PoolOfflineFetchError> PoolOfflineFetchError { get; set; } = null!;
        public virtual DbSet<PoolUpdate> PoolUpdate { get; set; } = null!;
        public virtual DbSet<PoolRelay> PoolRelay { get; set; } = null!;
        public virtual DbSet<AddressInfo> AddressInfo { get; set; } = null!;
        public virtual DbSet<Block> Block { get; set; } = null!;
        public virtual DbSet<SlotLeader> SlotLeader { get; set; } = null!;
        public virtual DbSet<Transaction> Transaction { get; set; } = null!;
        public virtual DbSet<TransactionOutput> TransactionOutput { get; set; } = null!;
        public virtual DbSet<MultiAsset> MultiAsset { get; set; } = null!;
        public virtual DbSet<MultiAssetTransactionOutput> MultiAssetTransactionOutput { get; set; } = null!;
        public virtual DbSet<TransactionInput> TransactionInput { get; set; } = null!;
        public virtual DbSet<Datum> Datum { get; set; } = null!;
        public virtual DbSet<Script> Script { get; set; } = null!;
        public virtual DbSet<CollateralTransactionInput> CollateralTransactionInput { get; set; } = null!;
        public virtual DbSet<CollateralTransactionOutput> CollateralTransactionOutput { get; set; } = null!;
        public virtual DbSet<ReferenceTransactionInput> ReferenceTransactionInput { get; set; } = null!;
        public virtual DbSet<Withdrawal> Withdrawal { get; set; } = null!;
        public virtual DbSet<MultiAssetTransactionMint> MultiAssetTransactionMint { get; set; } = null!;
        public virtual DbSet<TransactionMetadata> TransactionMetadata { get; set; } = null!;
        public virtual DbSet<StakeRegistration> StakeRegistration { get; set; } = null!;
        public virtual DbSet<StakeDeregistration> StakeDeregistration { get; set; } = null!;
        public virtual DbSet<Delegation> Delegation { get; set; } = null!;
        public virtual DbSet<Treasury> Treasury { get; set; } = null!;
        public virtual DbSet<Reserve> Reserve { get; set; } = null!;
        public virtual DbSet<PotTransfer> PotTransfer { get; set; } = null!;
        public virtual DbSet<ParamProposal> ParamProposal { get; set; } = null!;
        public virtual DbSet<PoolRetire> PoolRetire { get; set; } = null!;
        public virtual DbSet<Redeemer> Redeemer { get; set; } = null!;
        public virtual DbSet<RedeemerData> RedeemerData { get; set; } = null!;
        public virtual DbSet<StakeAddress> StakeAddress { get; set; } = null!;
        public virtual DbSet<PoolOwner> PoolOwner { get; set; } = null!;
        public virtual DbSet<MultiAssetCache> MultiAssetCache { get; set; } = null!;
        public virtual DbSet<MultiAssetAddressCache> MultiAssetAddressCache { get; set; } = null!;
        public virtual DbSet<AccountCache> AccountCache { get; set; } = null!;
        public virtual DbSet<Reward> Reward { get; set; } = null!;
        public virtual DbSet<ExtraKeyyWitness> ExtraKeyyWitness { get; set; } = null!;
        public virtual DbSet<CBIPoll> CBIPoll { get; set; } = null!;
        public virtual DbSet<CBIPoolParam> CBIPoolParam { get; set; } = null!;

        // BI Domain
        public virtual DbSet<PoolStat> PoolStat { get; set; } = null!;
        public virtual DbSet<AddressStat> AddressStat { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //             if (!optionsBuilder.IsConfigured)
            //             {
            // #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
            //                 optionsBuilder.UseNpgsql("Host=localhost;Database=cardanobi;Username=cardano;Password=cardano");
            //             }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum("rewardtype", new[] { "leader", "member", "reserves", "treasury", "refund" })
                .HasPostgresEnum("scriptpurposetype", new[] { "spend", "mint", "cert", "reward" })
                .HasPostgresEnum("scripttype", new[] { "multisig", "timelock", "plutusV1", "plutusV2" })
                .HasPostgresEnum("syncstatetype", new[] { "lagging", "following" });

            // Core Domain

            modelBuilder.Entity<AddressInfo>(entity =>
            {
                entity.ToView("_cbi_address_info_cache");
            });
            modelBuilder.Entity<EpochStakeView>(entity =>
            {
                entity.ToView("epoch_stake_view");
            });
            modelBuilder.Entity<EpochStakeView>().HasKey(c => new { c.epoch_stake_id });

            modelBuilder.Entity<MultiAssetAddressCache>().HasKey(c => new { c.asset_id, c.address });

            // Ignore derived fields for the relevant entities
            modelBuilder.Entity<EpochParam>().Ignore(e => e.nonce_hex);
            modelBuilder.Entity<PoolHash>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolMetadata>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolOfflineData>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolUpdate>().Ignore(e => e.vrf_key_hash_hex);
            modelBuilder.Entity<Block>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<Block>().Ignore(e => e.op_cert_hex);
            modelBuilder.Entity<SlotLeader>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<Transaction>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<TransactionOutput>().Ignore(e => e.data_hash_hex);
            modelBuilder.Entity<MultiAsset>().Ignore(e => e.policy_hex);
            modelBuilder.Entity<Datum>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<Script>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<CollateralTransactionOutput>().Ignore(e => e.data_hash_hex);
            modelBuilder.Entity<ParamProposal>().Ignore(e => e.key_hex);
            modelBuilder.Entity<Redeemer>().Ignore(e => e.script_hash_hex);
            modelBuilder.Entity<RedeemerData>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<RedeemerData>().Ignore(e => e.bytes_hex);
            modelBuilder.Entity<StakeAddress>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<StakeAddress>().Ignore(e => e.script_hash_hex);
            modelBuilder.Entity<ExtraKeyyWitness>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<CBIPoll>().Ignore(e => e.question_hash_hex);

            // BI Domain
            
            modelBuilder.Entity<PoolStat>(entity =>
                   {
                       entity.ToView("pool_stat_view");
                   });
            modelBuilder.Entity<PoolStat>().HasKey(c => new { c.epoch_no, c.pool_hash });

            modelBuilder.Entity<AddressStat>(entity =>
                   {
                       entity.ToView("_cbi_address_stats_cache");
                   });
            modelBuilder.Entity<AddressStat>().HasKey(c => new { c.epoch_no, c.address, c.stake_address_id });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
