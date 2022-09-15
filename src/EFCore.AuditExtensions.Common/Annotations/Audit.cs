using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Annotations.Trigger;

namespace EFCore.AuditExtensions.Common.Annotations;

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