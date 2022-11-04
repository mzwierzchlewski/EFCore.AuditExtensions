using EFCore.AuditExtensions.Common.Annotations.Table;

namespace EFCore.AuditExtensions.Common.Annotations.Trigger;

internal class AuditTrigger
{
    public string Name { get; }

    public string TableName { get; }

    public string AuditTableName { get; }

    public string AuditedEntityTableKeyColumnName { get; }

    public AuditColumnType AuditedEntityTableKeyColumnType { get; }
    
    public int UpdateOptimisationThreshold { get; }
    
    public bool NoKeyChanges { get; }

    public AuditTrigger(string name, string tableName, string auditTableName, string auditedEntityTableKeyColumnName, AuditColumnType auditedEntityTableKeyColumnType, int updateOptimisationThreshold, bool noKeyChanges)
    {
        Name = name;
        TableName = tableName;
        AuditTableName = auditTableName;
        AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName;
        AuditedEntityTableKeyColumnType = auditedEntityTableKeyColumnType;
        UpdateOptimisationThreshold = updateOptimisationThreshold;
        NoKeyChanges = noKeyChanges;
    }

    #region Comparers

    protected bool Equals(AuditTrigger other) => Name == other.Name && TableName == other.TableName && AuditTableName == other.AuditTableName && AuditedEntityTableKeyColumnName == other.AuditedEntityTableKeyColumnName && AuditedEntityTableKeyColumnType == other.AuditedEntityTableKeyColumnType && UpdateOptimisationThreshold == other.UpdateOptimisationThreshold && NoKeyChanges == other.NoKeyChanges;

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

    public override int GetHashCode() => HashCode.Combine(Name, TableName, AuditTableName, AuditedEntityTableKeyColumnName, (int)AuditedEntityTableKeyColumnType, UpdateOptimisationThreshold, NoKeyChanges);

    public static bool operator ==(AuditTrigger? left, AuditTrigger? right) => Equals(left, right);

    public static bool operator !=(AuditTrigger? left, AuditTrigger? right) => !Equals(left, right);

    #endregion
}