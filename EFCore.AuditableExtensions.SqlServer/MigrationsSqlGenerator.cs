using EFCore.AuditableExtensions.Common.Migrations.Operations;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.SqlServer;

internal class MigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
{
    private readonly ICreateAuditTriggerSqlGenerator _createAuditTriggerSqlGenerator;

    private readonly IDropAuditTriggerSqlGenerator _dropAuditTriggerSqlGenerator;

    public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations, ICreateAuditTriggerSqlGenerator createAuditTriggerSqlGenerator, IDropAuditTriggerSqlGenerator dropAuditTriggerSqlGenerator) : base(dependencies, migrationsAnnotations)
    {
        _createAuditTriggerSqlGenerator = createAuditTriggerSqlGenerator;
        _dropAuditTriggerSqlGenerator = dropAuditTriggerSqlGenerator;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case CreateAuditTriggerOperation createAuditTriggerOperation:
                _createAuditTriggerSqlGenerator.Generate(createAuditTriggerOperation, builder);
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