using EFCore.AuditExtensions.Common.SharedModels;

namespace EFCore.AuditExtensions.Common.Annotations.Trigger;

internal class AuditTrigger
{
    public string Name { get; }

    public string TableName { get; }

    public string AuditTableName { get; }
    
    public AuditedEntityKeyProperty[] KeyProperties { get; }
    
    public int UpdateOptimisationThreshold { get; }
    
    public bool NoKeyChanges { get; }

    public AuditTrigger(string name, string tableName, string auditTableName, AuditedEntityKeyProperty[] keyProperties, int updateOptimisationThreshold, bool noKeyChanges)
    {
        Name = name;
        TableName = tableName;
        AuditTableName = auditTableName;
        KeyProperties = keyProperties;
        UpdateOptimisationThreshold = updateOptimisationThreshold;
        NoKeyChanges = noKeyChanges;
    }

    #region Comparers

    protected bool Equals(AuditTrigger other)
        => Name == other.Name
           && TableName == other.TableName
           && AuditTableName == other.AuditTableName
           && UpdateOptimisationThreshold == other.UpdateOptimisationThreshold
           && NoKeyChanges == other.NoKeyChanges
           && KeyProperties.Length == other.KeyProperties.Length
           && KeyProperties.All(p => other.KeyProperties.Any(op => op.ColumnName == p.ColumnName && op.ColumnType == p.ColumnType));

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

    public override int GetHashCode() => HashCode.Combine(Name, TableName, AuditTableName, KeyProperties, UpdateOptimisationThreshold, NoKeyChanges);

    public static bool operator ==(AuditTrigger? left, AuditTrigger? right) => Equals(left, right);

    public static bool operator !=(AuditTrigger? left, AuditTrigger? right) => !Equals(left, right);

    #endregion
}