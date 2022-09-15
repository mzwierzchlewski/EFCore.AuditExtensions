using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class ReadOnlyEntityTypeExtensions
{
    public static (string name, Type type) GetSimpleKeyNameAndType(this IReadOnlyEntityType readOnlyEntityType)
    {
        var key = readOnlyEntityType.GetKeys().OrderByDescending(k => k.IsPrimaryKey()).FirstOrDefault(k => k.Properties.Count == 1);
        if (key == null)
        {
            return (string.Empty, typeof(object));
        }

        return (key.Properties[0].Name, key.Properties[0].ClrType);
    }
}