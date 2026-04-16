using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Flavian.Domain.Models.Demos;
using Flavian.Persistence.Data.Configurations.Common;

namespace Flavian.Persistence.Data.Configurations;

public class DemoAuditConfigurations : IEntityTypeConfiguration<DemoAudit>
{
    public void Configure(EntityTypeBuilder<DemoAudit> entity)
    {
        entity.ToTable("demo_audit", "dbo");

        entity.ConfigureBaseAuditEntity();

        entity.HasOne(demoAudit => demoAudit.Demo)
            .WithMany(demo => demo.Audits)
            .HasForeignKey(demoAudit => demoAudit.EventId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
