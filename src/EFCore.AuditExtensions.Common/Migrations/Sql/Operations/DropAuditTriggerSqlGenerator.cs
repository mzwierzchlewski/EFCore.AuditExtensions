using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.AuditExtensions.Common.Migrations.Sql.Operations;

internal interface IDropAuditTriggerSqlGenerator
{
    void Generate(DropAuditTriggerOperation operation, MigrationCommandListBuilder builder);
}