namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal class AuditTable
{
    public string Name { get; }

    public IReadOnlyCollection<AuditTableColumn> Columns { get; }

    public AuditTableIndex? Index { get; }

    public AuditTable(string name, IReadOnlyCollection<AuditTableColumn> columns, AuditTableIndex? index)
    {
        Columns = columns;
        Index = index;
        Name = name;
    }
}