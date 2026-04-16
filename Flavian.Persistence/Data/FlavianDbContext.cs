using Microsoft.EntityFrameworkCore;
using Flavian.Domain.Models.Demos;

namespace Flavian.Persistence.Data;

public class FlavianDbContext(DbContextOptions<FlavianDbContext> options) : DbContext(options)
{
    public DbSet<Demo> Demos { get; set; }
    public DbSet<DemoAudit> DemoAudits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FlavianDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<decimal>().HavePrecision(18, 6);
    }
}
