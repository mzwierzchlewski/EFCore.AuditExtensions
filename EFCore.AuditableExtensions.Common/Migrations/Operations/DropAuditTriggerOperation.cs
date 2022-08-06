using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.Common.Migrations.Operations;

public class DropAuditTriggerOperation : MigrationOperation
{
    public string TriggerName { get; }

    public DropAuditTriggerOperation(string triggerName)
    {
        TriggerName = triggerName;
    }
}