using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using EFCore.AuditExtensions.Common.SharedModels;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartFormat;

namespace EFCore.AuditExtensions.Common.Migrations.CSharp;

internal class CSharpMigrationOperationGenerator : Microsoft.EntityFrameworkCore.Migrations.Design.CSharpMigrationOperationGenerator
{
    private const string BaseCreateAuditTriggerCSharp =
$@".CreateAuditTrigger(
    auditedEntityTableName: ""{{AuditedEntityTableName}}"",
    auditTableName: ""{{AuditTableName}}"",
    triggerName: ""{{TriggerName}}"",
    auditedEntityTableKey: new {nameof(AuditedEntityKeyProperty)}[]
    \{{
{{AuditedEntityTableKey:list:        new(columnName: ""{{ColumnName}}"", columnType: {nameof(AuditColumnType)}.{{ColumnType}}{{MaxLength:isnull:|, maxLength: {{}}}})|,\n}}
    \}},
    updateOptimisationThreshold: {{UpdateOptimisationThreshold}},
    noKeyChanges: {{NoKeyChanges.ToString.ToLower}})";

    private const string BaseDropAuditTriggerCSharp = @".DropAuditTrigger(triggerName: ""{TriggerName}"")";

    public CSharpMigrationOperationGenerator(CSharpMigrationOperationGeneratorDependencies dependencies) : base(dependencies)
    { }

    protected override void Generate(MigrationOperation operation, IndentedStringBuilder builder)
    {
        if (operation == null)
        {
            throw new ArgumentNullException(nameof(operation));
        }

        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        switch (operation)
        {
            case CreateAuditTriggerOperation createAuditTriggerOperation:
                Generate(createAuditTriggerOperation, builder);
                break;

            case DropAuditTriggerOperation dropAuditTriggerOperation:
                Generate(dropAuditTriggerOperation, builder);
                break;

            default:
                base.Generate(operation, builder);
                break;
        }
    }

    private static void Generate(CreateAuditTriggerOperation operation, IndentedStringBuilder builder)
    {
        var csharpCode = Smart.Format(BaseCreateAuditTriggerCSharp, operation);
        builder.AppendLines(csharpCode, true);
    }

    private static void Generate(DropAuditTriggerOperation operation, IndentedStringBuilder builder)
    {
        var csharpCode = Smart.Format(BaseDropAuditTriggerCSharp, operation);
        builder.AppendLines(csharpCode, true);
    }
}