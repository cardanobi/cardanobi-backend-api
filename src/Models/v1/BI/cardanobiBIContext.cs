using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ApiCore.Models
{
    public partial class cardanobiBIContext : DbContext
    {
        // public cardanobiBIContext()
        // {
        // }

        public cardanobiBIContext(DbContextOptions<cardanobiBIContext> options)
            : base(options)
        {
        }

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

            modelBuilder.Entity<PoolStat>(entity =>
                   {
                       entity.ToView("pool_stat_view");
                   });
            modelBuilder.Entity<PoolStat>().HasKey(c => new { c.epoch_no, c.pool_hash });

            modelBuilder.Entity<AddressStat>(entity =>
                   {
                       entity.ToView("address_stat_view");
                   });
            modelBuilder.Entity<AddressStat>().HasKey(c => new { c.epoch_no, c.stake_address });

            // Ignore derived fields for the relevant entities
            // modelBuilder.Entity<EpochParam>().Ignore(e => e.nonce_hex);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
