using System.Text.RegularExpressions;
using EFCore.AuditExtensions.Common;
using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
using EFCore.AuditExtensions.Common.Migrations.Sql.Operations;
using EFCore.AuditExtensions.Common.SharedModels;
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
                INSERT INTO [{AuditTableName}] ({KeyColumnsNamesCsv}, [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
                SELECT {InsertKeyColumnSql}, (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{OldDataColumnName}], (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{NewDataColumnName}], 'UPDATE', @user, GETUTCDATE()
                FROM Deleted D {JoinKeyColumnSql} Inserted I ON {KeyColumnsJoinConditionSql};
            END;
            ELSE
            BEGIN
                -- Create table variables with inserted and deleted data
                -- and indexes on the key columns that will help with joins
                DECLARE @{AuditTableName}_Deleted TABLE (
                    {KeyColumnsTableDeclarationSql},
                    [{OldDataColumnName}] NVARCHAR(MAX)
                    PRIMARY KEY CLUSTERED ({KeyColumnsNamesCsv}));
                INSERT INTO @{AuditTableName}_Deleted
                SELECT {KeyColumnsNamesCsv}, (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{OldDataColumnName}]
                FROM Deleted D;

                DECLARE @{AuditTableName}_Inserted TABLE  (
                    {KeyColumnsTableDeclarationSql},
                    [{NewDataColumnName}] NVARCHAR(MAX)
                    PRIMARY KEY CLUSTERED ({KeyColumnsNamesCsv}));
                INSERT INTO @{AuditTableName}_Inserted
                SELECT {KeyColumnsNamesCsv}, (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [{NewDataColumnName}]
                FROM Inserted I;

                {CoalesceFullJoinComment}
                INSERT INTO [{AuditTableName}] ({KeyColumnsNamesCsv}, [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
                SELECT {InsertKeyColumnSql}, D.[{OldDataColumnName}], I.[{NewDataColumnName}], 'UPDATE', @user, GETUTCDATE()
                FROM @{AuditTableName}_Deleted D {JoinKeyColumnSql} @{AuditTableName}_Inserted I ON {KeyColumnsJoinConditionSql};
            END;
        END;
        -- Handle INSERT statements
        ELSE IF EXISTS(SELECT * FROM Inserted)
        BEGIN
            INSERT INTO [{AuditTableName}] ({KeyColumnsNamesCsv}, [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
            SELECT {KeyColumnsNamesCsv}, NULL, (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), 'INSERT', @user, GETUTCDATE()
            FROM Inserted I;
        END;
        -- Handle DELETE statements
        ELSE IF EXISTS(SELECT * FROM Deleted)
        BEGIN
            INSERT INTO [{AuditTableName}] ({KeyColumnsNamesCsv}, [{OldDataColumnName}], [{NewDataColumnName}], [{OperationTypeColumnName}], [{UserColumnName}], [{TimestampColumnName}])
            SELECT {KeyColumnsNamesCsv}, (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), NULL, 'DELETE', @user, GETUTCDATE()
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
            true => "{KeyColumns:list:D.[{Name}]|, }",
            false => "{KeyColumns:list:COALESCE(D.[{Name}], I.[{Name}])|, }",
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

        KeyColumn[] GetKeyColumns(AuditedEntityKeyProperty[] keyProperties)
        {
            var result = new List<KeyColumn>();
            foreach (var keyProperty in keyProperties)
            {
                var keyColumnTypeMapping = typeMappingSource.FindMapping(keyProperty.ColumnType.GetClrType(), storeTypeName: null, size: keyProperty.MaxLength) 
                                           ?? throw new ArgumentException("Column type is not supported");
                result.Add(new KeyColumn { Name = keyProperty.ColumnName, Type = keyColumnTypeMapping.StoreType });
            }

            return result.ToArray();
        }
        

        return new CreateAuditTriggerSqlParameters
        {
            TriggerName = operation.TriggerName,
            AuditedEntityTableName = operation.AuditedEntityTableName,
            AuditTableName = operation.AuditTableName,
            KeyColumns = GetKeyColumns(operation.AuditedEntityTableKey),
            KeyColumnsNamesCsv = "{KeyColumns:list:[{Name}]|, }",
            KeyColumnsJoinConditionSql = "{KeyColumns:list:D.[{Name}] = I.[{Name}]| AND }",
            KeyColumnsTableDeclarationSql = "{KeyColumns:list:[{Name}] {Type}|,\n                    }",
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

        public KeyColumn[]? KeyColumns { get; init; }
        
        public string? KeyColumnsNamesCsv { get; init; }
        
        public string? KeyColumnsJoinConditionSql { get; init; }
        
        public string? KeyColumnsTableDeclarationSql { get; init; }

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

    private class KeyColumn
    {
        public string? Name { get; init; }
        
        public string? Type { get; init; }
    }
}