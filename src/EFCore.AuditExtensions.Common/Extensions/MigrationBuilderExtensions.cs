using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace EFCore.AuditExtensions.Common.Extensions;

public static class MigrationBuilderExtensions
{
    public static OperationBuilder<CreateAuditTriggerOperation> CreateAuditTrigger(
        this MigrationBuilder migrationBuilder,
        string auditedEntityTableName,
        string auditTableName,
        string triggerName,
        AuditedEntityKeyProperty[] auditedEntityTableKey,
        int updateOptimisationThreshold,
        bool noKeyChanges)
    {
        var operation = new CreateAuditTriggerOperation(auditedEntityTableName, auditTableName, triggerName, auditedEntityTableKey, updateOptimisationThreshold, noKeyChanges);
        migrationBuilder.Operations.Add(operation);

        return new OperationBuilder<CreateAuditTriggerOperation>(operation);
    }

    public static OperationBuilder<DropAuditTriggerOperation> DropAuditTrigger(
        this MigrationBuilder migrationBuilder,
        string triggerName)
    {
        var operation = new DropAuditTriggerOperation(triggerName);
        migrationBuilder.Operations.Add(operation);

        return new OperationBuilder<DropAuditTriggerOperation>(operation);
    }
}