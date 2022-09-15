using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class EntityTypeExtensions
{
    public static IAnnotation GetAuditAnnotation(this IEntityType entityType) => entityType.GetAnnotations().Single(a => a.Name.StartsWith(Constants.AnnotationPrefix));
}