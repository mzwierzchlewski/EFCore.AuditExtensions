using EFCore.AuditableExtensions.Common.Migrations.CSharp.Operations;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.AuditableExtensions.Common.Migrations.Sql.Operations;

internal interface IDropAuditTriggerSqlGenerator
{
    void Generate(DropAuditTriggerOperation operation, MigrationCommandListBuilder builder);
}