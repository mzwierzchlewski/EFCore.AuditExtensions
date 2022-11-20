using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using EFCore.AuditExtensions.Common.Migrations.Sql.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;

namespace EFCore.AuditExtensions.SqlServer.SqlGenerators;

internal class SqlServerMigrationsSqlGenerator : Microsoft.EntityFrameworkCore.Migrations.SqlServerMigrationsSqlGenerator
{
    private readonly ICreateAuditTriggerSqlGenerator _createAuditTriggerSqlGenerator;

    private readonly IDropAuditTriggerSqlGenerator _dropAuditTriggerSqlGenerator;

    public SqlServerMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer commandBatchPreparer, ICreateAuditTriggerSqlGenerator createAuditTriggerSqlGenerator, IDropAuditTriggerSqlGenerator dropAuditTriggerSqlGenerator) : base(dependencies, commandBatchPreparer)
    {
        _createAuditTriggerSqlGenerator = createAuditTriggerSqlGenerator;
        _dropAuditTriggerSqlGenerator = dropAuditTriggerSqlGenerator;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case CreateAuditTriggerOperation createAuditTriggerOperation:
                _createAuditTriggerSqlGenerator.Generate(createAuditTriggerOperation, builder, Dependencies.TypeMappingSource);
                break;

            case DropAuditTriggerOperation dropAuditTriggerOperation:
                _dropAuditTriggerSqlGenerator.Generate(dropAuditTriggerOperation, builder);
                break;

            default:
                base.Generate(operation, model, builder);
                break;
        }
    }
}