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
    FOR INSERT, UPDATE, DELETE AS
    BEGIN
        IF @@ROWCOUNT = 0 RETURN;
        SET NOCOUNT ON;
        DECLARE @user varchar(255);
        SET @user = COALESCE(CAST(SESSION_CONTEXT(N'user') AS VARCHAR(255)), CONCAT(SUSER_NAME(), ' [db]'));

        -- Handle UPDATE statements
        IF EXISTS(SELECT * FROM Inserted) AND EXISTS(SELECT * FROM Deleted)
        BEGIN
            IF @@ROWCOUNT < {UpdateOptimisationThreshold}
            BEGIN
                INSERT INTO [{AuditTableName}] ([{KeyColumnName}], [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
                SELECT {InsertKeyColumnSql}, (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{OldDataColumnName}], (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{NewDataColumnName}], 'UPDATE', @user, GETUTCDATE()
                FROM Deleted D {JoinKeyColumnSql} Inserted I ON D.[{KeyColumnName}] = I.[{KeyColumnName}];
            END;
            ELSE
            BEGIN
                -- Create temporal tables with inserted and deleted data so that key column can be indexed
                -- and joins will be less painful
                DECLARE @{AuditTableName}_Deleted TABLE (
                    [{KeyColumnName}] {KeyColumnType} PRIMARY KEY CLUSTERED,
                    [{OldDataColumnName}] NVARCHAR(MAX));
                INSERT INTO @{AuditTableName}_Deleted
                SELECT [{KeyColumnName}], (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{OldDataColumnName}]
                FROM Deleted D;

                DECLARE @{AuditTableName}_Inserted TABLE  (
                    [{KeyColumnName}] {KeyColumnType} PRIMARY KEY CLUSTERED,
                    [{NewDataColumnName}] NVARCHAR(MAX));
                INSERT INTO @{AuditTableName}_Inserted
                SELECT [{KeyColumnName}], (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{NewDataColumnName}]
                FROM Inserted I;

                {CoalesceFullJoinComment}
                INSERT INTO [{AuditTableName}] ([{KeyColumnName}], [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
                SELECT {InsertKeyColumnSql}, D.[{OldDataColumnName}], I.[{NewDataColumnName}], 'UPDATE', @user, GETUTCDATE()
                FROM @{AuditTableName}_Deleted D {JoinKeyColumnSql} @{AuditTableName}_Inserted I ON D.[{KeyColumnName}] = I.[{KeyColumnName}];
            END;
        END;
        -- Handle INSERT statements
        ELSE IF EXISTS(SELECT * FROM Inserted)
        BEGIN
            INSERT INTO [{AuditTableName}] ([{KeyColumnName}], [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
            SELECT [{KeyColumnName}], NULL, (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), 'INSERT', @user, GETUTCDATE()
            FROM Inserted I;
        END;
        -- Handle DELETE statements
        ELSE IF EXISTS(SELECT * FROM Deleted)
        BEGIN
            INSERT INTO [{AuditTableName}] ([{KeyColumnName}], [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
            SELECT [{KeyColumnName}], (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), NULL, 'DELETE', @user, GETUTCDATE()
            FROM Deleted D;
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
        string GetInsertKeyColumnSql(bool noKeyChanges) => noKeyChanges switch
        {
            true => "D.[{KeyColumnName}]",
            false => "COALESCE(D.[{KeyColumnName}], I.[{KeyColumnName}])",
        };

        string GetJoinKeyColumnSql(bool noKeyChanges) => noKeyChanges switch
        {
            true  => "INNER JOIN",
            false => "FULL OUTER JOIN",
        };

        string GetCoalesceFullJoinComment(bool noKeyChanges) => noKeyChanges switch
        {
            true  => string.Empty,
            false => "-- COALESCE and FULL OUTER JOIN prevent loss of data when value of the primary key was changed",
        };
        
        var keyColumnTypeMapping = typeMappingSource.FindMapping(operation.AuditedEntityTableKeyColumnType.GetClrType()) ?? throw new ArgumentException("Column type is not supported");

        return new CreateAuditTriggerSqlParameters
        {
            TriggerName = operation.TriggerName,
            AuditedEntityTableName = operation.AuditedEntityTableName,
            AuditTableName = operation.AuditTableName,
            KeyColumnName = operation.AuditedEntityTableKeyColumnName,
            KeyColumnType = keyColumnTypeMapping.StoreType,
            UpdateOptimisationThreshold = operation.UpdateOptimisationThreshold,
            InsertKeyColumnSql = GetInsertKeyColumnSql(operation.NoKeyChanges),
            JoinKeyColumnSql = GetJoinKeyColumnSql(operation.NoKeyChanges),
            CoalesceFullJoinComment = GetCoalesceFullJoinComment(operation.NoKeyChanges),
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

        public string? AuditTableName { get; init; }

        public string? KeyColumnName { get; init; }

        public string? KeyColumnType { get; init; }

        public string? OldDataColumnName { get; init; }

        public string? NewDataColumnName { get; init; }

        public string? OperationTypeColumnName { get; init; }

        public string? UserColumnName { get; init; }

        public string? TimestampColumnName { get; init; }
        
        public int? UpdateOptimisationThreshold { get; init; }
        
        public string? InsertKeyColumnSql { get; init; }
        
        public string? JoinKeyColumnSql { get; init; }
        
        public string? CoalesceFullJoinComment { get; init; }
    }
}