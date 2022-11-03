namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal class AuditTableIndex
{
    public string? Name { get; }

    public string ColumnName { get; }

    public AuditColumnType ColumnType { get; }

    public AuditTableIndex(string? name, string columnName, AuditColumnType columnType)
    {
        Name = name;
        ColumnName = columnName;
        ColumnType = columnType;
    }
}