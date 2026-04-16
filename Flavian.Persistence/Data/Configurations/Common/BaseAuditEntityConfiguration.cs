using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Flavian.Domain.Models.Common;

namespace Flavian.Persistence.Data.Configurations.Common;

public static class BaseAuditEntityConfiguration
{
    public static void ConfigureBaseAuditEntity<TEntity>(this EntityTypeBuilder<TEntity> entity)
        where TEntity : BaseAuditEntity
    {
        entity.HasKey(baseAuditEntity => baseAuditEntity.Id);
        entity.Property(baseAuditEntity => baseAuditEntity.Id).HasColumnName("id").ValueGeneratedNever();
        entity.Property(baseAuditEntity => baseAuditEntity.EventId).HasColumnName("event_id").IsRequired();
        entity.Property(baseAuditEntity => baseAuditEntity.ActionName).HasColumnName("action_name").HasMaxLength(50).IsRequired();
        entity.Property(baseAuditEntity => baseAuditEntity.ActionDetails).HasColumnName("action_details").HasMaxLength(1000).IsRequired();
        entity.Property(baseAuditEntity => baseAuditEntity.CreatedBy).HasColumnName("created_by").IsRequired();
        entity.Property(baseAuditEntity => baseAuditEntity.CreatedDate).HasColumnName("created_date").HasDefaultValueSql("GETDATE()").IsRequired();
        entity.Property(baseAuditEntity => baseAuditEntity.RowId).HasColumnName("row_id").ValueGeneratedOnAdd();

        var rowIdMeta = entity.Property(baseAuditEntity => baseAuditEntity.RowId).Metadata;
        rowIdMeta.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
        rowIdMeta.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
    }
}
