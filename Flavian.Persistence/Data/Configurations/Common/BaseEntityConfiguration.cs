using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Flavian.Domain.Models.Common;

namespace Flavian.Persistence.Data.Configurations.Common;

public static class BaseEntityConfiguration
{
    public static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : BaseEntity
    {
        entity.HasKey(baseEntity => baseEntity.Id);

        entity.Property(baseEntity => baseEntity.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(baseEntity => baseEntity.RowId).HasColumnName("row_id").ValueGeneratedOnAdd();

        var rowIdMeta = entity.Property(baseEntity => baseEntity.RowId).Metadata;
        rowIdMeta.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        rowIdMeta.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        entity.Property(baseEntity => baseEntity.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false).IsRequired();
        entity.Property(baseEntity => baseEntity.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(baseEntity => baseEntity.BusinessUnit).HasColumnName("business_unit").IsRequired();
        entity.Property(baseEntity => baseEntity.CreatedDate).HasColumnName("created_date").HasDefaultValueSql("GETDATE()").IsRequired();
        entity.Property(baseEntity => baseEntity.ModifiedBy).HasColumnName("modified_by");
        entity.Property(baseEntity => baseEntity.ModifiedDate).HasColumnName("modified_date");
    }
}
