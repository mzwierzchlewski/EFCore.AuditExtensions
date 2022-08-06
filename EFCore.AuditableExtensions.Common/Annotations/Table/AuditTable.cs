namespace EFCore.AuditableExtensions.Common.Annotations.Table;

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