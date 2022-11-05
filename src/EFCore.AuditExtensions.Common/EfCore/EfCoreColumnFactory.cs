using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.AuditExtensions.Common.EfCore;

#pragma warning disable EF1001

internal static class EfCoreColumnFactory
{
    public static Column ToEfCoreColumn(this AuditTableColumn auditTableColumn, Table table, TableMapping tableMapping, EntityType entityType, IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        var columnClrType = auditTableColumn.Type.GetClrType();
        var columnMaxLength = auditTableColumn.MaxLength;
        var columnTypeMapping = relationalTypeMappingSource.FindMapping(type: columnClrType, storeTypeName: null, size: columnMaxLength) ?? throw new ArgumentException("Column type is not supported");
        var tableColumn = new Column(auditTableColumn.Name, columnTypeMapping.StoreType, table)
        {
            IsNullable = auditTableColumn.Nullable,
        };
        var columnMapping = new ColumnMapping(new Property(auditTableColumn.Name, columnClrType, null, null, entityType, ConfigurationSource.Explicit, null), tableColumn, tableMapping);
        tableColumn.PropertyMappings.Add(columnMapping);

        return tableColumn;
    }
}

#pragma warning restore EF1001