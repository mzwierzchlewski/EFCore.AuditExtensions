using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.EfCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

namespace EFCore.AuditExtensions.Common.EfCore;

#pragma warning disable EF1001

internal static class EfCoreTableIndexFactory
{
    public static CustomAuditTableIndex ToEfCoreCustomTableIndex(this AuditTableIndex auditTableIndex, EntityType auditEntityType, Table table, IEnumerable<Column> columns)
    {
        var columnsArray = columns.ToArray();
        if (!string.IsNullOrEmpty(auditTableIndex.Name))
        {
            return new CustomAuditTableIndex(auditTableIndex.Name, table, columnsArray);
        }

        var indexName = GetDefaultIndexName(auditEntityType, columnsArray);
        return new CustomAuditTableIndex(indexName, table, columnsArray);
    }

    private static string GetDefaultIndexName(EntityType auditEntityType, IEnumerable<Column> columns)
    {
        var properties = columns.Select(c => new Property(c.Name, typeof(object), null, null, auditEntityType, ConfigurationSource.Explicit, ConfigurationSource.Explicit)).ToArray();
        var index = new Index(properties, auditEntityType, ConfigurationSource.Explicit);
        return index.GetDatabaseName() ?? throw new InvalidOperationException("Failed to determine audit table index name");
    }
}

#pragma warning restore EF1001