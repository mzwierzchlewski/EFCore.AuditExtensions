using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.EfCore.Models;
using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

namespace EFCore.AuditExtensions.Common.EfCore;

#pragma warning disable EF1001

internal static class EfCoreTableIndexFactory
{
    public static CustomAuditTableIndex ToEfCoreCustomTableIndex(this AuditTableIndex auditTableIndex, EntityType auditEntityType, Table table, Column column)
    {
        if (!string.IsNullOrEmpty(auditTableIndex.Name))
        {
            return new CustomAuditTableIndex(auditTableIndex.Name, table, new[] { column });
        }

        var indexName = auditTableIndex.GetDefaultIndexName(auditEntityType);
        return new CustomAuditTableIndex(indexName, table, new[] { column });
    }

    private static string GetDefaultIndexName(this AuditTableIndex auditTableIndex, EntityType auditEntityType)
    {
        var property = new Property(auditTableIndex.ColumnName, auditTableIndex.ColumnType.GetClrType(), null, null, auditEntityType, ConfigurationSource.Explicit, ConfigurationSource.Explicit);
        var index = new Index(new[] { property }, auditEntityType, ConfigurationSource.Explicit);
        return index.GetDatabaseName() ?? throw new InvalidOperationException("Failed to determine audit table index name");
    }
}

#pragma warning restore EF1001