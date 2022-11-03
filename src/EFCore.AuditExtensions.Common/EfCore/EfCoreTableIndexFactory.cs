using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.EfCore.Models;
using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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

        var indexName = auditTableIndex.GetIndexName(auditEntityType);
        return new CustomAuditTableIndex(indexName, table, new[] { column });
    }

    private static string GetIndexName(this AuditTableIndex auditTableIndex, IReadOnlyEntityType auditEntityType)
    {
        var model = new Model();
        var entityType = model.AddEntityType(auditEntityType.Name, auditEntityType.IsOwned(), ConfigurationSource.Explicit) ?? throw new InvalidOperationException("Failed to create audit entity type for audit table index");
        var property = entityType.AddProperty(auditTableIndex.ColumnName, auditTableIndex.ColumnType.GetClrType(), ConfigurationSource.Explicit, ConfigurationSource.Explicit) ?? throw new InvalidOperationException("Failed to create audit entity property for audit table index");
        var index = entityType.AddIndex(property, ConfigurationSource.Explicit) ?? throw new InvalidOperationException("Failed to create audit entity index for audit table index");
        var storeObject = StoreObjectIdentifier.Create(auditEntityType, StoreObjectType.Table) ?? throw new InvalidOperationException("Failed to create store object identifier for audit table index");
        return index.GetDatabaseName(storeObject) ?? throw new InvalidOperationException("Failed to determine audit table index name");
    }
}

#pragma warning restore EF1001