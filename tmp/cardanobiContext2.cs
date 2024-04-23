using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ApiCore;

public partial class cardanobiContext : DbContext
{
    public cardanobiContext()
    {
    }

    public cardanobiContext(DbContextOptions<cardanobiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<_cbi_active_stake_cache_account> _cbi_active_stake_cache_accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=cardanobi;Username=cardano;Password=Cardano2023");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("rewardtype", new[] { "leader", "member", "reserves", "treasury", "refund" })
            .HasPostgresEnum("scriptpurposetype", new[] { "spend", "mint", "cert", "reward" })
            .HasPostgresEnum("scripttype", new[] { "multisig", "timelock", "plutusV1", "plutusV2" })
            .HasPostgresEnum("syncstatetype", new[] { "lagging", "following" });

        modelBuilder.Entity<_cbi_active_stake_cache_account>(entity =>
        {
            entity.HasKey(e => new { e.stake_address_id, e.pool_hash_id, e.epoch_no }).HasName("_cbi_active_stake_cache_account_pkey");

            entity.Property(e => e.amount).HasDefaultValueSql("0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
