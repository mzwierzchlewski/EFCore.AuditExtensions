using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.AuditExtensions.Common.EfCore.Models;

#pragma warning disable EF1001

public class CustomAuditTableIndex : TableIndex
{
    public override string? Filter => MappedIndexes.FirstOrDefault()?.GetFilter(StoreObjectIdentifier.Table(Table.Name, Table.Schema));

    public CustomAuditTableIndex(string name, Table table, IReadOnlyList<Column> columns) : base(name, table, columns, false)
    { }
}

#pragma warning restore EF1001