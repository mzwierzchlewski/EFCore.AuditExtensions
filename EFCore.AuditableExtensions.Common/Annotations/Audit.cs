using EFCore.AuditableExtensions.Common.Annotations.Table;
using EFCore.AuditableExtensions.Common.Annotations.Trigger;

namespace EFCore.AuditableExtensions.Common.Annotations;

internal class Audit
{
    public string Name { get; }

    public AuditTable Table { get; }

    public IReadOnlyCollection<AuditTrigger> Triggers { get; }

    public Audit(string name, AuditTable table, IReadOnlyCollection<AuditTrigger> triggers)
    {
        Name = name;
        Table = table;
        Triggers = triggers;
    }
}