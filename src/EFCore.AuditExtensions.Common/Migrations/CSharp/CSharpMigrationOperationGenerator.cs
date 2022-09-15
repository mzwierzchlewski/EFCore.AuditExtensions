using System.Data;
using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartFormat;

namespace EFCore.AuditExtensions.Common.Migrations.CSharp;

internal class CSharpMigrationOperationGenerator : Microsoft.EntityFrameworkCore.Migrations.Design.CSharpMigrationOperationGenerator
{
    private const string BaseCreateAuditTriggerCSharp =
        $@".CreateAuditTrigger(
        ""{{AuditedEntityTableName}}"",
        ""{{AuditTableName}}"",
        ""{{TriggerName}}"",
        {nameof(StatementType)}.{{OperationType}},
        ""{{AuditedEntityTableKeyColumnName}}"",
        {nameof(AuditColumnType)}.{{AuditedEntityTableKeyColumnType}})";

    private const string BaseDropAuditTriggerCSharp = @".DropAuditTrigger(""{TriggerName}"")";

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