namespace EFCore.AuditableExtensions.Common.Annotations.Table;

internal class AuditTable
{
    public string Name { get; }

    public IReadOnlyCollection<AuditTableColumn> Columns { get; }

    public AuditTable(string name, IReadOnlyCollection<AuditTableColumn> columns)
    {
        Columns = columns;
        Name = name;
    }
}