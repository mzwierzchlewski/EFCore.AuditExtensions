using System.Data;
using System.Text.RegularExpressions;
using EFCore.AuditExtensions.Common;
using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using EFCore.AuditExtensions.Common.Migrations.Sql.Operations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using SmartFormat;

namespace EFCore.AuditExtensions.SqlServer.SqlGenerators.Operations;

internal class CreateAuditTriggerSqlGenerator : ICreateAuditTriggerSqlGenerator
{
    private const string BaseSql = @"
    CREATE TRIGGER [{TriggerName}] ON [{AuditedEntityTableName}]
    FOR {OperationType} AS
    BEGIN
        IF @@ROWCOUNT = 0 RETURN;
        SET NOCOUNT ON;
        IF {ExistsSql}
        BEGIN
            DECLARE @user varchar(255);
            SET @user = COALESCE(CAST(SESSION_CONTEXT(N'user') AS VARCHAR(255)), CONCAT(SUSER_NAME(), ' [db]'));

            DECLARE @{KeyColumnName} {KeyColumnType};
            DECLARE @C CURSOR;

            SET @C = CURSOR LOCAL FAST_FORWARD FOR 
            SELECT [{KeyColumnName}] FROM {CursorSource};

            OPEN @C;
            FETCH NEXT FROM @C INTO @{KeyColumnName};
                
            WHILE @@FETCH_STATUS = 0
            BEGIN
                INSERT INTO [{AuditTableName}] (
                    [{KeyColumnName}],
                    [{OldDataColumnName}],
                    [{NewDataColumnName}],
                    [{OperationTypeColumnName}],
                    [{UserColumnName}],
                    [{TimestampColumnName}]
                )
                VALUES(
                    @{KeyColumnName},
                    {OldDataSql},
                    {NewDataSql},
                    '{OperationType}',
                    @user,
                    GETUTCDATE()
                );
                FETCH NEXT FROM @C INTO @{KeyColumnName};
            END;
        END;
    END;";

    private static readonly Regex PlaceholderRegex = new("{[A-Za-z]+}", RegexOptions.Compiled);

    public void Generate(CreateAuditTriggerOperation operation, MigrationCommandListBuilder builder, IRelationalTypeMappingSource typeMappingSource)
    {
        var sqlParameters = GetSqlParameters(operation, typeMappingSource);
        foreach (var sqlLine in BaseSql.Split('\n'))
        {
            builder.Append(ReplacePlaceholders(sqlLine, sqlParameters));
        }

        builder.EndCommand();
    }

    private static string ReplacePlaceholders(string sql, CreateAuditTriggerSqlParameters parameters)
    {
        var result = sql;
        while (PlaceholderRegex.IsMatch(result))
        {
            result = Smart.Format(result, parameters);
        }

        return result;
    }

    private static CreateAuditTriggerSqlParameters GetSqlParameters(CreateAuditTriggerOperation operation, IRelationalTypeMappingSource typeMappingSource)
    {
        string GetExistsSql(StatementType statementType) => statementType switch
        {
            StatementType.Insert => "EXISTS(SELECT * FROM Inserted)",
            StatementType.Update => "EXISTS(SELECT * FROM Inserted) AND EXISTS(SELECT * FROM Deleted)",
            StatementType.Delete => "EXISTS(SELECT * FROM Deleted)",
            _                    => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        string GetCursorSource(StatementType statementType) => statementType switch
        {
            StatementType.Insert or StatementType.Update => "Inserted",
            StatementType.Delete                         => "Deleted",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        string GetOldDataSql(StatementType statementType) => statementType switch
        {
            StatementType.Insert                         => "null",
            StatementType.Update or StatementType.Delete => "(SELECT * FROM Deleted WHERE [{KeyColumnName}] = @{KeyColumnName} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        string GetNewDataSql(StatementType statementType) => statementType switch
        {
            StatementType.Insert or StatementType.Update => "(SELECT * FROM Inserted WHERE [{KeyColumnName}] = @{KeyColumnName} FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)",
            StatementType.Delete                         => "null",
            _                                            => throw new ArgumentOutOfRangeException(nameof(statementType), statementType, "Value not supported"),
        };

        var keyColumnTypeMapping = typeMappingSource.FindMapping(operation.AuditedEntityTableKeyColumnType.GetClrType()) ?? throw new ArgumentException("Column type is not supported");

        return new CreateAuditTriggerSqlParameters
        {
            TriggerName = operation.TriggerName,
            AuditedEntityTableName = operation.AuditedEntityTableName,
            AuditTableName = operation.AuditTableName,
            OperationType = operation.OperationType.ToString().ToUpper(),
            KeyColumnName = operation.AuditedEntityTableKeyColumnName,
            KeyColumnType = keyColumnTypeMapping.StoreType,
            OldDataSql = GetOldDataSql(operation.OperationType),
            NewDataSql = GetNewDataSql(operation.OperationType),
            ExistsSql = GetExistsSql(operation.OperationType),
            CursorSource = GetCursorSource(operation.OperationType),
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

        public string? CursorSource { get; init; }

        public string? OldDataSql { get; init; }

        public string? NewDataSql { get; init; }

        public string? ExistsSql { get; init; }

        public string? KeyColumnName { get; init; }

        public string? KeyColumnType { get; init; }

        public string? OldDataColumnName { get; init; }

        public string? NewDataColumnName { get; init; }

        public string? OperationTypeColumnName { get; init; }

        public string? UserColumnName { get; init; }

        public string? TimestampColumnName { get; init; }
    }
}