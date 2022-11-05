using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;

public class CreateAuditTriggerOperation : MigrationOperation, IDependentMigrationOperation
{
    public string TriggerName { get; }

    public string AuditedEntityTableName { get; }
    
    public AuditedEntityKeyProperty[] AuditedEntityTableKey { get; }
    
    public string AuditTableName { get; }
    
    public int UpdateOptimisationThreshold { get; }
    
    public bool NoKeyChanges { get; }

    public Type[] DependsOn { get; } = { typeof(MigrationBuilderExtensions), typeof(AuditColumnType), typeof(AuditedEntityKeyProperty) };

    public CreateAuditTriggerOperation(string auditedEntityTableName, string auditTableName, string triggerName, AuditedEntityKeyProperty[] auditedEntityTableKey, int updateOptimisationThreshold, bool noKeyChanges)
    {
        AuditedEntityTableName = auditedEntityTableName;
        AuditTableName = auditTableName;
        TriggerName = triggerName;
        AuditedEntityTableKey = auditedEntityTableKey;
        UpdateOptimisationThreshold = updateOptimisationThreshold;
        NoKeyChanges = noKeyChanges;
    }
}