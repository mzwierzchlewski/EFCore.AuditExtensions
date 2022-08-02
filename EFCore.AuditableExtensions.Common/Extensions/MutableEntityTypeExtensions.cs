using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class MutableEntityTypeExtensions
{
    public static (string name, Type type) GetSimpleKeyNameAndType(this IMutableEntityType mutableEntityType)
    {
        var key = mutableEntityType.GetKeys().OrderByDescending(k => k.IsPrimaryKey()).FirstOrDefault(k => k.Properties.Count == 1);
        if (key == null)
        {
            return (string.Empty, typeof(object));
        }

        return (key.Properties[0].Name, key.Properties[0].ClrType);
    }
}