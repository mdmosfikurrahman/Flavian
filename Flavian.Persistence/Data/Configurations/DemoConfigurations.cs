using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Flavian.Domain.Models.Demos;
using Flavian.Persistence.Data.Configurations.Common;

namespace Flavian.Persistence.Data.Configurations;

public class DemoConfigurations : IEntityTypeConfiguration<Demo>
{
    public void Configure(EntityTypeBuilder<Demo> entity)
    {
        entity.ToTable("demo", "dbo");
        entity.ConfigureBaseEntity();

        entity.Property(demo => demo.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        entity.Property(demo => demo.Description).HasColumnName("description").HasMaxLength(255);
        entity.Property(demo => demo.IsActive).HasColumnName("is_active").HasDefaultValue(true).IsRequired();
    }
}
