namespace EFCore.AuditableExtensions.Common.Annotations.Table;

#pragma warning disable EF1001

internal interface IAuditTable
{
    string Name { get; }

    IReadOnlyCollection<AuditTableColumn> Columns { get; }
}

internal class AuditTable : IAuditTable
{
    public string Name { get; }

    public IReadOnlyCollection<AuditTableColumn> Columns { get; }

    public AuditTable(string name, IReadOnlyCollection<AuditTableColumn> columns)
    {
        Columns = columns;
        Name = name;
    }
}

#pragma warning restore EF1001