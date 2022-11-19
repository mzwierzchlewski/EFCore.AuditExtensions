using EFCore.AuditExtensions.Common.Annotations;
using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Annotations.Trigger;
using EFCore.AuditExtensions.Common.Configuration;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.AuditExtensions.Common.Extensions;

public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Instructs Entity Framework to create the infrastructure necessary for logging entity changes.
    /// </summary>
    /// <param name="entityTypeBuilder">EntityTypeBuilder of the entity that should be audited.</param>
    /// <param name="configureOptions">Action over AuditOptions allowing for customisation of the auditing infrastructure.</param>
    /// <typeparam name="T">Type of the entity that should be audited.</typeparam>
    /// <returns>EntityTypeBuilder for further chaining.</returns>
    public static EntityTypeBuilder<T> IsAudited<T>(this EntityTypeBuilder<T> entityTypeBuilder, Action<AuditOptions<T>>? configureOptions = null)
        where T : class
    {
        entityTypeBuilder.AddDelayedAuditAnnotation(
            entityType =>
            {
                var auditOptions = AuditOptionsFactory.GetConfiguredAuditOptions(configureOptions);
                var auditTable = AuditTableFactory.CreateFromEntityType(entityType, auditOptions);
                var auditTrigger = AuditTriggerFactory.CreateFromAuditTableAndEntityType(auditTable, entityType, auditOptions);
                var auditName = $"{Constants.AnnotationPrefix}:{typeof(T).Name}";
                var audit = new Audit(auditName, auditTable, auditTrigger);
                entityTypeBuilder.AddAuditAnnotation(audit);
            });

        return entityTypeBuilder;
    }

    private static EntityTypeBuilder<T> AddDelayedAuditAnnotation<T>(this EntityTypeBuilder<T> entityTypeBuilder, Action<IMutableEntityType> addAuditAction) 
        where T : class
    {
        entityTypeBuilder.GetEntityType().AddAnnotation(Constants.AddAuditAnnotationName, addAuditAction);

        return entityTypeBuilder;
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