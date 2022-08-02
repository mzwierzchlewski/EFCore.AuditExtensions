using EFCore.AuditableExtensions.Common.Configuration;
using EFCore.AuditableExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditableExtensions.Common.Annotations.Table;

internal static class AuditTableFactory
{
    private static AuditTableColumn[] GetDefaultColumns() => new[]
    {
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OldData, true),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.NewData, true),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OperationType, false),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.User, false),
        new AuditTableColumn(AuditColumnType.DateTime, Constants.AuditTableColumnNames.Timestamp, false),
    };

    private static AuditTableColumn GetKeyColumn<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        string keyName;
        Type keyType;
        if (options.AuditedEntityKeySelector == null)
        {
            (keyName, keyType) = entityType.GetSimpleKeyNameAndType();
            if (string.IsNullOrEmpty(keyName))
            {
                throw new InvalidOperationException("Auditable entity must either have a simple Key or the AuditedEntityKeySelector must be provided");
            }
        }
        else
        {
            (keyName, keyType) = options.AuditedEntityKeySelector.GetAccessedPropertyNameAndType();
            if (string.IsNullOrEmpty(keyName))
            {
                throw new InvalidOperationException("AuditedEntityKeySelector must point to a single property");
            }
        }

        return new AuditTableColumn(keyType.GetAuditColumnType(), keyName, false);
    }

    private static IReadOnlyCollection<AuditTableColumn> GetColumnsForEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var columns = new List<AuditTableColumn>
        {
            GetKeyColumn(entityType, options),
        };
        columns.AddRange(GetDefaultColumns());

        return columns.ToArray();
    }
    
    private static string GetNameForEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class 
        => string.IsNullOrEmpty(options.AuditTableName) ? $"{entityType.GetTableName()}{Constants.AuditTableNameSuffix}" : options.AuditTableName;

    public static AuditTable CreateFromEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var columns = GetColumnsForEntityType(entityType, options);
        var name = GetNameForEntityType(entityType, options);

        return new AuditTable(name, columns);
    }
}