using EFCore.AuditableExtensions.Common.Migrations.Operations;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace EFCore.AuditableExtensions.SqlServer;

internal class MigrationsSqlGenerator : SqlServerMigrationsSqlGenerator
{
    private readonly ICreateAuditTriggerSqlGenerator _createAuditTriggerSqlGenerator;

    public MigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider migrationsAnnotations, ICreateAuditTriggerSqlGenerator createAuditTriggerSqlGenerator) : base(dependencies, migrationsAnnotations)
    {
        _createAuditTriggerSqlGenerator = createAuditTriggerSqlGenerator;
    }

    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case CreateAuditTriggerOperation createAuditTriggerOperation:
                _createAuditTriggerSqlGenerator.Generate(createAuditTriggerOperation, builder);
                break;
                break;

            default:
                base.Generate(operation, model, builder);
                break;
        }
    }
}