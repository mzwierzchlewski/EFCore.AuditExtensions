using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;

public class CreateAuditTriggerOperation : MigrationOperation, IDependentMigrationOperation
{
    public string TriggerName { get; }

    public string AuditedEntityTableName { get; }

    public string AuditedEntityTableKeyColumnName { get; }

    public AuditColumnType AuditedEntityTableKeyColumnType { get; }

    public string AuditTableName { get; }
    
    public int UpdateOptimisationThreshold { get; }
    
    public bool NoKeyChanges { get; }

    public Type[] DependsOn { get; } = { typeof(MigrationBuilderExtensions), typeof(AuditColumnType) };

    public CreateAuditTriggerOperation(string auditedEntityTableName, string auditTableName, string triggerName, string auditedEntityTableKeyColumnName, AuditColumnType auditedEntityTableKeyColumnType, int updateOptimisationThreshold, bool noKeyChanges)
    {
        AuditedEntityTableName = auditedEntityTableName;
        AuditTableName = auditTableName;
        TriggerName = triggerName;
        AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName;
        AuditedEntityTableKeyColumnType = auditedEntityTableKeyColumnType;
        UpdateOptimisationThreshold = updateOptimisationThreshold;
        NoKeyChanges = noKeyChanges;
    }
}