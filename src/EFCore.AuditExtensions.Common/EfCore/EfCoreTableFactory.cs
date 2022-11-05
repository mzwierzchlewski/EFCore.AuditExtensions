using EFCore.AuditExtensions.Common.Annotations.Table;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.AuditExtensions.Common.EfCore;

#pragma warning disable EF1001

internal static class EfCoreTableFactory
{
    public static Table ToEfCoreTable(this AuditTable auditTable, RelationalModel relationalModel, IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        var table = new Table(auditTable.Name, null, relationalModel);
        var model = relationalModel.Model as Model ?? throw new ArgumentException("Invalid Model property in RelationalModel argument", nameof(relationalModel));
        var auditEntityType = new EntityType(auditTable.Name, model, false, ConfigurationSource.Explicit);
        var tableMapping = new TableMapping(auditEntityType, table, false)
        {
            IsSharedTablePrincipal = true,
        };
        table.EntityTypeMappings.Add(tableMapping);

        var indexColumns = new List<Column>(auditTable.Columns.Count(c => c.AuditedEntityKey));
        foreach (var auditTableColumn in auditTable.Columns)
        {
            var column = auditTableColumn.ToEfCoreColumn(table, tableMapping, auditEntityType, relationalTypeMappingSource);
            table.Columns.Add(column.Name, column);

            if (auditTableColumn.AuditedEntityKey && auditTable.Index != null)
            {
                indexColumns.Add(column);
            }
        }

        if (auditTable.Index != null)
        {
            if (!indexColumns.Any())
            {
                throw new InvalidOperationException("Cannot define audit table index because no key columns were found");
            }
            
            var tableIndex = auditTable.Index.ToEfCoreCustomTableIndex(auditEntityType, table, indexColumns);
            table.Indexes.Add(tableIndex.Name, tableIndex);
        }

        return table;
    }
}

#pragma warning restore EF1001