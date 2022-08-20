using System.Data;
using EFCore.AuditableExtensions.Common.Annotations.Table;
using EFCore.AuditableExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.Common.Migrations.CSharp.Operations;

public class CreateAuditTriggerOperation : MigrationOperation, IDependentMigrationOperation
{
    public string TriggerName { get; }

    public StatementType OperationType { get; }

    public string AuditedEntityTableName { get; }

    public string AuditedEntityTableKeyColumnName { get; }

    public AuditColumnType AuditedEntityTableKeyColumnType { get; }

    public string AuditTableName { get; }

    public Type[] DependsOn { get; } = { typeof(MigrationBuilderExtensions), typeof(StatementType), typeof(AuditColumnType) };

    public CreateAuditTriggerOperation(string auditedEntityTableName, string auditTableName, string triggerName, StatementType operationType, string auditedEntityTableKeyColumnName, AuditColumnType auditedEntityTableKeyColumnType)
    {
        AuditedEntityTableName = auditedEntityTableName;
        AuditTableName = auditTableName;
        TriggerName = triggerName;
        OperationType = operationType;
        AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName;
        AuditedEntityTableKeyColumnType = auditedEntityTableKeyColumnType;
    }
}