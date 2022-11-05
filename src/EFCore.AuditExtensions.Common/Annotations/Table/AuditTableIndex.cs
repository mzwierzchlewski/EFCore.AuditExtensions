namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal class AuditTableIndex
{
    public string? Name { get; }
    
    public AuditTableIndex(string? name)
    {
        Name = name;
    }
}