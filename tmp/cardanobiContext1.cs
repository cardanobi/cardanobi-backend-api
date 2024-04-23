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

    public virtual DbSet<_cbi_active_stake_cache_pool> _cbi_active_stake_cache_pools { get; set; }

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

        modelBuilder.Entity<_cbi_active_stake_cache_pool>(entity =>
        {
            entity.HasKey(e => new { e.pool_id, e.epoch_no }).HasName("_cbi_active_stake_cache_pool_pkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
