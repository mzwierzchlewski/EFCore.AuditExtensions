namespace EFCore.AuditableExtensions.Common.Annotations.Table;

internal class AuditTableColumn
{
    public AuditColumnType Type { get; }

    public string Name { get; }

    public bool Nullable { get; }

    public AuditTableColumn(AuditColumnType type, string name, bool nullable)
    {
        Type = type;
        Name = name;
        Nullable = nullable;
    }
}