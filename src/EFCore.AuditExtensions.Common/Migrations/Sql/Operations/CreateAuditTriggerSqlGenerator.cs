using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.AuditExtensions.Common.Migrations.Sql.Operations;

internal interface ICreateAuditTriggerSqlGenerator
{
    void Generate(CreateAuditTriggerOperation operation, MigrationCommandListBuilder builder, IRelationalTypeMappingSource typeMappingSource);
}