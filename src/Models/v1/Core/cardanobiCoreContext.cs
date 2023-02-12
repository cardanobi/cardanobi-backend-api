using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        public virtual DbSet<Epoch> Epoch { get; set; } = null!;
        public virtual DbSet<EpochParam> EpochParam { get; set; } = null!;
        public virtual DbSet<EpochStake> EpochStake { get; set; } = null!;
        public virtual DbSet<PoolHash> PoolHash { get; set; } = null!;
        public virtual DbSet<PoolMetadata> PoolMetadata { get; set; } = null!;
        public virtual DbSet<PoolOfflineData> PoolOfflineData { get; set; } = null!;
        public virtual DbSet<PoolOfflineFetchError> PoolOfflineFetchError { get; set; } = null!;
        public virtual DbSet<PoolUpdate> PoolUpdate { get; set; } = null!;
        public virtual DbSet<PoolRelay> PoolRelay { get; set; } = null!;
        public virtual DbSet<AddressInfo> AddressInfo { get; set; } = null!;

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

            modelBuilder.Entity<AddressInfo>(entity =>
            {
                entity.ToView("address_info_view");
            });
            modelBuilder.Entity<EpochStake>(entity =>
            {
                entity.ToView("epoch_stake_view");
            });

            // Ignore derived fields for the relevant entities
            modelBuilder.Entity<EpochParam>().Ignore(e => e.nonce_hex);
            modelBuilder.Entity<PoolHash>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolMetadata>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolOfflineData>().Ignore(e => e.hash_hex);
            modelBuilder.Entity<PoolUpdate>().Ignore(e => e.vrf_key_hash_hex);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
