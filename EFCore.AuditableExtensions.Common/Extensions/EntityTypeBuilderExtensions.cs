using EFCore.AuditableExtensions.Common.Annotations;
using EFCore.AuditableExtensions.Common.Annotations.Table;
using EFCore.AuditableExtensions.Common.Annotations.Trigger;
using EFCore.AuditableExtensions.Common.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.AuditableExtensions.Common.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<T> IsAudited<T>(this EntityTypeBuilder<T> entityTypeBuilder, Action<AuditOptions<T>>? configureOptions = null)
        where T : class
    {
        var entityType = entityTypeBuilder.GetEntityType();
        var auditOptions = AuditOptionsFactory.GetConfiguredAuditOptions(configureOptions);
        var auditTable = AuditTableFactory.CreateFromEntityType(entityType, auditOptions);
        var auditTriggers = AuditTriggerFactory.CreateFromAuditTableAndEntityType(auditTable, entityType, auditOptions);
        var auditName = $"{Constants.AnnotationPrefix}:{nameof(T)}";
        var audit = new Audit(auditName, auditTable, auditTriggers);
        return entityTypeBuilder.AddAuditAnnotation(audit);
    }

    private static IMutableEntityType GetEntityType<T>(this EntityTypeBuilder<T> entityTypeBuilder) where T : class
    {
        var entityType = entityTypeBuilder.Metadata.Model.FindEntityType(typeof(T).FullName!);
        if (entityType == null)
        {
            throw new InvalidOperationException("Entity type is missing from model");
        }

        return entityType;
    }

    private static EntityTypeBuilder<T> AddAuditAnnotation<T>(this EntityTypeBuilder<T> entityTypeBuilder, Audit audit) where T : class
    {
        var entityType = entityTypeBuilder.GetEntityType();
        entityType.AddAnnotation(audit.Name, audit.Serialize());
        return entityTypeBuilder;
    }
}