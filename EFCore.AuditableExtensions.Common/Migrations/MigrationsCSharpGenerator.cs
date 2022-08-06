using System.Data;
using EFCore.AuditableExtensions.Common.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartFormat;

namespace EFCore.AuditableExtensions.Common.Migrations;

internal class MigrationsCSharpGenerator : CSharpMigrationOperationGenerator
{
    private const string BaseCreateAuditTriggerCSharp = $@"
        .CreateAuditTrigger(
        ""{{AuditedEntityTableName}}"",
        ""{{AuditTableName}}"",
        ""{{TriggerName}}"",
        {nameof(StatementType)}.{{OperationType}},
        ""{{AuditedEntityTableKeyColumnName}}"")";

    private const string BaseDropAuditTriggerCSharp = @".DropAuditTrigger(""{TriggerName}"")";

    public MigrationsCSharpGenerator(CSharpMigrationOperationGeneratorDependencies dependencies) : base(dependencies)
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