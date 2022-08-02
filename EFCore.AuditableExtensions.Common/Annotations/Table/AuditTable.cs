using EFCore.AuditableExtensions.Common.Configuration;
using Newtonsoft.Json;

namespace EFCore.AuditableExtensions.Common.Annotations.Table;

#pragma warning disable EF1001

internal interface IAuditTable
{
    string Name { get; }

    IReadOnlyCollection<AuditTableColumn> Columns { get; }
}

internal class AuditTable<T> : IAuditTable where T : class
{
    public string Name { get; }

    public IReadOnlyCollection<AuditTableColumn> Columns { get; }

    public AuditTable(IReadOnlyCollection<AuditTableColumn> columns, AuditOptions<T> options)
    {
        Columns = columns;
        Name = options.AuditTableName;
    }
}

internal class SimpleAuditTable : IAuditTable
{
    public string Name { get; }

    public IReadOnlyCollection<AuditTableColumn> Columns { get; }

    [JsonConstructor]
    public SimpleAuditTable(string name, IReadOnlyCollection<AuditTableColumn> columns)
    {
        Name = name;
        Columns = columns;
    }
}

#pragma warning restore EF1001