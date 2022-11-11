using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class ModelBuilderExtensions
{
    public static IReadOnlyCollection<(IMutableEntityType EntityType, IAnnotation Annotation)> GetEntityTypesWithDelayedAuditAnnotation(this ModelBuilder modelBuilder)
        => modelBuilder.Model.GetEntityTypes()
                       .Where(et => et.HasAnnotation(Constants.AddAuditAnnotationName))
                       .Select(et => (EntityType: et, Annotation: et.GetAnnotation(Constants.AddAuditAnnotationName)))
                       .ToArray();
}