using System.Data;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.Common.Migrations.Operations;

public class CreateAuditTriggerOperation : MigrationOperation
{
    public string TriggerName { get; }

    public StatementType OperationType { get; }

    public string AuditedEntityTableName { get; }

    public string AuditedEntityTableKeyColumnName { get; }

    public string AuditTableName { get; }

    public CreateAuditTriggerOperation(string auditedEntityTableName, string auditTableName, string triggerName, StatementType operationType, string auditedEntityTableKeyColumnName)
    {
        AuditedEntityTableName = auditedEntityTableName;
        AuditTableName = auditTableName;
        TriggerName = triggerName;
        OperationType = operationType;
        AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName;
    }
}