using System.Data;
using EFCore.AuditableExtensions.Common.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal enum GetNamespace
{
    MigrationBuilderExtensions,
}

public static class MigrationBuilderExtensions
{
    public static OperationBuilder<CreateAuditTriggerOperation> CreateAuditTrigger(
        this MigrationBuilder migrationBuilder,
        string auditedEntityTableName,
        string auditTableName,
        string triggerName,
        StatementType operationType,
        string auditedEntityTableKeyColumnName)
    {
        var operation = new CreateAuditTriggerOperation(auditedEntityTableName, auditTableName, triggerName, operationType, auditedEntityTableKeyColumnName);
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