using System.Text.Json.Serialization;

namespace EFCore.AuditExtensions.Common.Annotations.Table;

internal class AuditTableColumn
{
    public AuditColumnType Type { get; }

    public string Name { get; }

    public bool Nullable { get; }

    public bool AuditedEntityKey { get; }

    [JsonConstructor]
    public AuditTableColumn(AuditColumnType type, string name, bool nullable, bool auditedEntityKey)
    {
        Type = type;
        Name = name;
        Nullable = nullable;
        AuditedEntityKey = auditedEntityKey;
    }
}