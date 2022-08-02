using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class ModelExtensions
{
    public static IReadOnlyCollection<IEntityType> GetAuditedEntityTypes(this IModel? model)
    {
        if (model == null)
        {
            return Array.Empty<IEntityType>();
        }

        return model.GetEntityTypes().Where(et => et.GetAnnotations().Any(a => a.Name.StartsWith(Constants.AnnotationPrefix))).ToArray();
    }
}