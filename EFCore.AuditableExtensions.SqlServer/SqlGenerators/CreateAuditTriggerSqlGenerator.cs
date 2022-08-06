using System.Data;
using EFCore.AuditableExtensions.Common;
using EFCore.AuditableExtensions.Common.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartFormat;

namespace EFCore.AuditableExtensions.SqlServer.SqlGenerators;

internal interface ICreateAuditTriggerSqlGenerator
{
    void Generate(CreateAuditTriggerOperation operation, MigrationCommandListBuilder builder);
}

internal class CreateAuditTriggerSqlGenerator : ICreateAuditTriggerSqlGenerator
{
    private const string BaseSql = @"
    CREATE TRIGGER [{TriggerName}] ON [{AuditedEntityTableName}]
    FOR {OperationType} AS
    BEGIN
    DECLARE @user varchar(255)
    SELECT @user = COALESCE(CAST(SESSION_CONTEXT(N'user') AS VARCHAR(255)), CONCAT(SUSER_NAME(), ' [db]'))
     
    INSERT INTO [{AuditTableName}] (
        [{KeyColumnName}],
        [{OldDataColumnName}],
        [{NewDataColumnName}],
        [{OperationTypeColumnName}],
        [{UserColumnName}],
        [{TimestampColumnName}]
    )
    VALUES(
        (SELECT {KeyColumnName} FROM {KeySource}),
        {OldDataSql},
        {NewDataSql},
        '{OperationType}',
        @user,
        GETUTCDATE()
    );
    END";

    public void Generate(CreateAuditTriggerOperation operation, MigrationCommandListBuilder builder)
    {
        var sqlParameters = GetSqlParameters(operation);
        foreach (var sqlLine in BaseSql.Split('\n').Where(line => !string.IsNullOrEmpty(line)))
        {
            builder.AppendLine(ReplacePlaceholders(sqlLine, sqlParameters));
        }

        builder.EndCommand();
    }

    private static string ReplacePlaceholders(string sql, CreateAuditTriggerSqlParameters parameters) => Smart.Format(sql, parameters);

    private static CreateAuditTriggerSqlParameters GetSqlParameters(CreateAuditTriggerOperation operation)
    {
        string GetKeySource(StatementType statementType) => statementType switch
        {
            StatementType.Insert or StatementType.Update => "Inserted",
            StatementType.Delete                         => "Deleted",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        string GetOldDataSql(StatementType statementType) => statementType switch
        {
            StatementType.Insert                         => "null",
            StatementType.Update or StatementType.Delete => "(SELECT * FROM Deleted FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        string GetNewDataSql(StatementType statementType) => statementType switch
        {
            StatementType.Insert or StatementType.Update => "(SELECT * FROM Inserted FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)",
            StatementType.Delete                         => "null",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        return new CreateAuditTriggerSqlParameters
        {
            TriggerName = operation.TriggerName,
            AuditedEntityTableName = operation.AuditedEntityTableName,
            AuditTableName = operation.AuditTableName,
            OperationType = operation.OperationType.ToString().ToUpper(),
            KeyColumnName = operation.AuditedEntityTableKeyColumnName,
            OldDataSql = GetOldDataSql(operation.OperationType),
            NewDataSql = GetNewDataSql(operation.OperationType),
            KeySource = GetKeySource(operation.OperationType),
            OldDataColumnName = Constants.AuditTableColumnNames.OldData,
            NewDataColumnName = Constants.AuditTableColumnNames.NewData,
            OperationTypeColumnName = Constants.AuditTableColumnNames.OperationType,
            UserColumnName = Constants.AuditTableColumnNames.User,
            TimestampColumnName = Constants.AuditTableColumnNames.Timestamp,
        };
    }

    private class CreateAuditTriggerSqlParameters
    {
        public string? TriggerName { get; init; }

        public string? AuditedEntityTableName { get; init; }

        public string? OperationType { get; init; }

        public string? AuditTableName { get; init; }

        public string? KeySource { get; init; }

        public string? OldDataSql { get; init; }
        
        public string? NewDataSql { get; init; }

        public string? KeyColumnName { get; init; }

        public string? OldDataColumnName { get; init; }

        public string? NewDataColumnName { get; init; }

        public string? OperationTypeColumnName { get; init; }

        public string? UserColumnName { get; init; }

        public string? TimestampColumnName { get; init; }
    }
}
