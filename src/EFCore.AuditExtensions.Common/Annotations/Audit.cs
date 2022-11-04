using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Annotations.Trigger;

namespace EFCore.AuditExtensions.Common.Annotations;

internal class Audit
{
    public string Name { get; }

    public AuditTable Table { get; }

    public AuditTrigger Trigger { get; }

    public Audit(string name, AuditTable table, AuditTrigger trigger)
    {
        Name = name;
        Table = table;
        Trigger = trigger;
    }
}