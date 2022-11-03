using System.Linq.Expressions;

namespace EFCore.AuditExtensions.Common.Configuration;

public class AuditOptions<TEntity> where TEntity : class
{
    public string? AuditTableName { get; set; }

    public string? AuditTriggerNameFormat { get; set; }

    public AuditKeyOptions<TEntity> AuditedEntityKeyOptions { get; } = new();
}

public class AuditKeyOptions<TEntity> where TEntity : class
{
    public Expression<Func<TEntity, object?>>? KeySelector { get; set; }

    public bool? Index { get; set; }

    public string? IndexName { get; set; }
}