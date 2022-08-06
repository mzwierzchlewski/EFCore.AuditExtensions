using EFCore.AuditableExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.Common.Migrations.Operations;

public class DropAuditTriggerOperation : MigrationOperation, IDependentMigrationOperation
{
    public string TriggerName { get; }

    public Type[] DependsOn { get; } = { typeof(MigrationBuilderExtensions) };

    public DropAuditTriggerOperation(string triggerName)
    {
        TriggerName = triggerName;
    }
}