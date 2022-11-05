using EFCore.AuditExtensions.Common.Configuration;
using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal static class AuditTableFactory
{
    public static AuditTable CreateFromEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var columns = GetColumnsForEntityType(entityType, options);
        var name = GetNameForEntityType(entityType, options);
        var index = GetIndexFromOptions(options);

        return new AuditTable(name, columns, index);
    }

    private static AuditTableColumn[] GetDefaultColumns(int? dataColumnsMaxLength) => new[]
    {
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OldData, true, false, dataColumnsMaxLength),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.NewData, true, false, dataColumnsMaxLength),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.OperationType, false, false, Constants.AuditTableColumnMaxLengths.OperationType),
        new AuditTableColumn(AuditColumnType.Text, Constants.AuditTableColumnNames.User, false, false, Constants.AuditTableColumnMaxLengths.User),
        new AuditTableColumn(AuditColumnType.DateTime, Constants.AuditTableColumnNames.Timestamp, false, false),
    };

    private static AuditTableColumn[] GetKeyColumns<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        IEnumerable<AuditedEntityKeyProperty> keyProperties;

        if (options.AuditedEntityKeyOptions.KeySelector == null)
        {
            keyProperties = entityType.GetKeyProperties();
            if (!keyProperties.Any())
            {
                throw new InvalidOperationException("Audited entity must either have a simple Key or the AuditedEntityKeySelector must be provided");
            }
        }
        else
        {
            keyProperties = options.AuditedEntityKeyOptions.KeySelector.GetKeyProperties(entityType);
            if (!keyProperties.Any())
            {
                throw new InvalidOperationException("AuditedEntityKeySelector must point to valid properties");
            }
        }

        return keyProperties.Select(property => new AuditTableColumn(property.ColumnType, property.ColumnName, false, true, property.MaxLength)).ToArray();
    }

    private static IReadOnlyCollection<AuditTableColumn> GetColumnsForEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var columns = new List<AuditTableColumn>();
        columns.AddRange(GetKeyColumns(entityType, options));
        columns.AddRange(GetDefaultColumns(options.DataColumnsMaxLength));

        return columns.ToArray();
    }

    private static string GetNameForEntityType<T>(IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
        => string.IsNullOrEmpty(options.AuditTableName) ? $"{entityType.GetTableName()}{Constants.AuditTableNameSuffix}" : options.AuditTableName;

    private static AuditTableIndex? GetIndexFromOptions<T>(AuditOptions<T> options) where T : class
    {
        if (options.AuditedEntityKeyOptions.KeySelector == null)
        {
            if (options.AuditedEntityKeyOptions.Index == false)
            {
                return null;
            }

            return new AuditTableIndex(options.AuditedEntityKeyOptions.IndexName);
        }

        if (options.AuditedEntityKeyOptions.Index is null or false)
        {
            return null;
        }

        return new AuditTableIndex(options.AuditedEntityKeyOptions.IndexName);
    }
}