using EFCore.AuditableExtensions.Common.Annotations.Table;
using EFCore.AuditableExtensions.Common.Annotations.Trigger;
using EFCore.AuditableExtensions.Common.Configuration;
using Newtonsoft.Json;

namespace EFCore.AuditableExtensions.Common.Annotations;

internal class Audit<T> : IAudit where T : class
{
    private readonly AuditTable<T> _table;

    public IReadOnlyCollection<AuditTrigger> Triggers { get; }


    public string Name => $"{Constants.AnnotationPrefix}:{typeof(T).Name}";

    public IAuditTable Table => _table;

    public Audit(AuditTable<T> table, IReadOnlyCollection<AuditTrigger> triggers, AuditOptions<T> options)
    {
        _table = table;
        Triggers = triggers;
    }
}

internal interface IAudit
{
    string Name { get; }

    IAuditTable Table { get; }
}

internal class SimpleAudit : IAudit
{
    public string Name { get; }

    public IAuditTable Table { get; }

    [JsonConstructor]
    public SimpleAudit(string name, SimpleAuditTable table, Type auditedEntityType)
    {
        Name = name;
        Table = table;
    }
}