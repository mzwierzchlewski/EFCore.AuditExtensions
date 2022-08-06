using System.Data;

namespace EFCore.AuditableExtensions.Common.Annotations.Trigger;

internal class AuditTrigger
{
    public string Name { get; }

    public string TableName { get; }

    public string AuditTableName { get; }

    public string AuditedEntityTableKeyColumnName { get; }

    public StatementType OperationType { get; }

    public AuditTrigger(string name, string tableName, string auditTableName, string auditedEntityTableKeyColumnName, StatementType operationType)
    {
        Name = name;
        TableName = tableName;
        AuditTableName = auditTableName;
        AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName;
        OperationType = operationType;
    }

    #region Comparers

    protected bool Equals(AuditTrigger other) => Name == other.Name && TableName == other.TableName && AuditTableName == other.AuditTableName && AuditedEntityTableKeyColumnName == other.AuditedEntityTableKeyColumnName && OperationType == other.OperationType;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((AuditTrigger)obj);
    }

    public override int GetHashCode() => HashCode.Combine(Name, TableName, AuditTableName, AuditedEntityTableKeyColumnName, (int)OperationType);

    public static bool operator ==(AuditTrigger? left, AuditTrigger? right) => Equals(left, right);

    public static bool operator !=(AuditTrigger? left, AuditTrigger? right) => !Equals(left, right);

    #endregion
}