using EFCore.AuditExtensions.Common.Configuration;
using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal static class AuditTableFactory
{
    public static AuditTable CreateFromEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var columns = GetColumnsForEntityType(entityType, options);
        var name = GetNameForEntityType(entityType, options);
        var index = GetIndexFromColumns(columns, options);

        return new AuditTable(name, columns, index);
    }

    private static AuditTableColumn[] GetDefaultColumns() => new[]
    {
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OldData, true, false),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.NewData, true, false),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OperationType, false, false),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.User, false, false),
        new AuditTableColumn(AuditColumnType.DateTime, Constants.AuditTableColumnNames.Timestamp, false, false),
    };

    private static AuditTableColumn GetKeyColumn<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        string keyName;
        Type keyType;

        if (options.AuditedEntityKeyOptions.KeySelector == null)
        {
            (keyName, keyType) = entityType.GetSimpleKeyNameAndType();
            if (string.IsNullOrEmpty(keyName))
            {
                throw new InvalidOperationException("Audited entity must either have a simple Key or the AuditedEntityKeySelector must be provided");
            }
        }
        else
        {
            (keyName, keyType) = options.AuditedEntityKeyOptions.KeySelector.GetAccessedPropertyNameAndType();
            if (string.IsNullOrEmpty(keyName))
            {
                throw new InvalidOperationException("AuditedEntityKeySelector must point to a single property");
            }
        }

        return new AuditTableColumn(keyType.GetAuditColumnType(), keyName, false, true);
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

    private static AuditTableIndex? GetIndexFromColumns<T>(IReadOnlyCollection<AuditTableColumn> columns, AuditOptions<T> options) where T : class
    {
        var keyColumn = columns.SingleOrDefault(c => c.AuditedEntityKey);
        if (keyColumn == null)
        {
            return null;
        }

        if (options.AuditedEntityKeyOptions.KeySelector == null)
        {
            if (options.AuditedEntityKeyOptions.Index == false)
            {
                return null;
            }

            return new AuditTableIndex(options.AuditedEntityKeyOptions.IndexName, keyColumn.Name, keyColumn.Type);
        }

        if (options.AuditedEntityKeyOptions.Index is null or false)
        {
            return null;
        }

        return new AuditTableIndex(options.AuditedEntityKeyOptions.IndexName, keyColumn.Name, keyColumn.Type);
    }
}