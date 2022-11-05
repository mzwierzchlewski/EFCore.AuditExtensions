using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class ReadOnlyEntityTypeExtensions
{
    public static IReadOnlyCollection<AuditedEntityKeyProperty> GetKeyProperties(this IReadOnlyEntityType readOnlyEntityType)
    {
        var key = readOnlyEntityType.GetKeys().OrderBy(k => k.IsPrimaryKey()).ThenBy(k => k.Properties.Count).FirstOrDefault();
        if (key == null)
        {
            return Array.Empty<AuditedEntityKeyProperty>();
        }

        var storeObject = StoreObjectIdentifier.Table(readOnlyEntityType.GetTableName()!);
        return key.Properties.Select(property => new AuditedEntityKeyProperty(
                                         property.GetColumnName(storeObject)!, 
                                         property.ClrType.GetAuditColumnType(), 
                                         property.GetMaxLength(storeObject)!)).ToArray();
    }
}