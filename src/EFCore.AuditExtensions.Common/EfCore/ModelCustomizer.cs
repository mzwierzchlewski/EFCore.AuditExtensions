using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.EfCore;

public class ModelCustomizer : RelationalModelCustomizer
{
    public ModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
    { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        HandleAuditAnnotations(modelBuilder);
    }

    private static void HandleAuditAnnotations(ModelBuilder modelBuilder)
    {
        var entityTypesWithAuditAnnotations = modelBuilder.GetEntityTypesWithDelayedAuditAnnotation();
        foreach (var (entityType, annotation) in entityTypesWithAuditAnnotations)
        {
            if (annotation.Value is not Action<IMutableEntityType> addAuditAction)
            {
                throw new InvalidOperationException($"Invalid {annotation.Name} annotation value for entity type: {entityType.Name}");
            }

            addAuditAction(entityType);
            entityType.RemoveAnnotation(annotation.Name);
        }
    }
}