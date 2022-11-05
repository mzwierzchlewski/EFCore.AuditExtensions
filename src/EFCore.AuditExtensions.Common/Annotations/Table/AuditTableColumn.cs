namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal class AuditTableColumn
{
    public AuditColumnType Type { get; }

    public string Name { get; }

    public bool Nullable { get; }

    public bool AuditedEntityKey { get; }
    
    public int? MaxLength { get; }
    
    public AuditTableColumn(AuditColumnType type, string name, bool nullable, bool auditedEntityKey, int? maxLength = null)
    {
        Type = type;
        Name = name;
        Nullable = nullable;
        AuditedEntityKey = auditedEntityKey;
        MaxLength = maxLength;
    }
}