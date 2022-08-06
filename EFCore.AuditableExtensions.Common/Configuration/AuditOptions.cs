using System.Linq.Expressions;

namespace EFCore.AuditableExtensions.Common.Configuration;

public class AuditOptions<TEntity> where TEntity : class
{
    public string? AuditTableName { get; set; }

    public Expression<Func<TEntity, object?>>? AuditedEntityKeySelector { get; set; }

    public string? AuditTriggerNameFormat { get; set; }
}