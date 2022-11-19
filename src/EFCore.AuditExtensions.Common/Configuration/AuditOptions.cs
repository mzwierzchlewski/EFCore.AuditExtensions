using System.Linq.Expressions;

namespace EFCore.AuditExtensions.Common.Configuration;

public class AuditOptions<TEntity> where TEntity : class
{
    /// <summary>
    /// Name of the audit table.
    /// Defaults to <value>[AuditedEntityTableName]_Audit</value>.
    /// </summary>
    public string? AuditTableName { get; set; }
    
    /// <summary>
    /// Maximum length of OldData and NewData columns.
    /// Defaults to EF Core's defaults for string type.
    /// </summary>
    public int? DataColumnsMaxLength { get; set; }

    /// <summary>
    /// Settings related to the audit trigger.
    /// </summary>
    public AuditTriggerOptions AuditTriggerOptions { get; } = new();
    
    /// <summary>
    /// Settings related to the audited entity's key.
    /// </summary>
    public AuditKeyOptions<TEntity> AuditedEntityKeyOptions { get; } = new();
}

public class AuditKeyOptions<TEntity> where TEntity : class
{
    /// <summary>
    /// Lambda expression specifying the properties that make up the primary key.
    /// Defaults to the audited entity's key.
    /// </summary>
    public Expression<Func<TEntity, object?>>? KeySelector { get; set; }

    /// <summary>
    /// Decides whether an index is created on the columns making up the primary key.
    /// Defaults to <value>true</value> if <see cref="KeySelector"/> is <code>null</code> and <value>false</value> otherwise.
    /// </summary>
    public bool? Index { get; set; }

    /// <summary>
    /// Name of the audit index.
    /// Defaults to EF Core's default name for index (<value>IX_[TableName]_[Property1]_[Property2]</value>).
    /// </summary>
    public string? IndexName { get; set; }
}

public class AuditTriggerOptions
{
    /// <summary>
    /// Format of the audit trigger name.
    /// Defaults to <value>Audit_[AuditedEntityTableName]_[AuditTableName]</value>.
    /// </summary>
    public string? NameFormat { get; set; }
    
    /// <summary>
    /// Threshold over which indexed table variables will be used when handling UPDATE statements.
    /// Defaults to <value>100</value>.
    /// </summary>
    public int? UpdateOptimisationThreshold { get; set; }
    
    /// <summary>
    /// Decides whether to use simpler but more performant UPDATE statement handling.
    /// Setting this to <value>true</value> is a declaration that the entity's key values will not change.
    /// If set to true and an entity's key is changed, some information about the UPDATE will be lost.
    /// Defaults to <value>false</value>.
    /// </summary>
    public bool? NoKeyChanges { get; set; }
}