using System.Linq.Expressions;

namespace EFCore.AuditExtensions.Common.Configuration;

public class AuditOptions<TEntity> where TEntity : class
{
    public string? AuditTableName { get; set; }

    public AuditTriggerOptions<TEntity> AuditTriggerOptions { get; } = new();
    public AuditKeyOptions<TEntity> AuditedEntityKeyOptions { get; } = new();
}

public class AuditKeyOptions<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, object?>>? KeySelector { get; set; }

    public bool? Index { get; set; }

    public string? IndexName { get; set; }
}

public class AuditTriggerOptions<TEntity> where TEntity : class
{
    public string? NameFormat { get; set; }
    
    public int? UpdateOptimisationThreshold { get; set; }
    
    public bool? NoKeyChanges { get; set; }
}