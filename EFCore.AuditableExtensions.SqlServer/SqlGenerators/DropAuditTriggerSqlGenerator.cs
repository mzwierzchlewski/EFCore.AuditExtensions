using EFCore.AuditableExtensions.Common.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartFormat;

namespace EFCore.AuditableExtensions.SqlServer.SqlGenerators;

internal interface IDropAuditTriggerSqlGenerator
{
    void Generate(DropAuditTriggerOperation operation, MigrationCommandListBuilder builder);
}

internal class DropAuditTriggerSqlGenerator : IDropAuditTriggerSqlGenerator
{
    private const string BaseSql = @"DROP TRIGGER IF EXISTS {TriggerName}";

    public void Generate(DropAuditTriggerOperation operation, MigrationCommandListBuilder builder)
    {
        builder.Append(ReplacePlaceholders(BaseSql, operation));
        builder.EndCommand();
    }

    private static string ReplacePlaceholders(string sql, DropAuditTriggerOperation operation)
    {
        var parameters = GetSqlParameters(operation);
        return Smart.Format(sql, parameters);
    }

    private static DropAuditTriggerSqlParameters GetSqlParameters(DropAuditTriggerOperation operation)
        => new()
        {
            TriggerName = operation.TriggerName,
        };

    private class DropAuditTriggerSqlParameters
    {
        public string? TriggerName { get; init; }
    }
}