using System.Data;
using EFCore.AuditableExtensions.Common.Annotations.Table;
using EFCore.AuditableExtensions.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartFormat;

namespace EFCore.AuditableExtensions.Common.Annotations.Trigger;

internal static class AuditTriggerFactory
{
    private const string DefaultTriggerNameFormat = "{AuditPrefix}_{TableName}_{AuditTableName}_{AuditedEntityTableKeyColumnName}_{OperationType}";

    private static readonly StatementType[] TriggerOperations =
    {
        StatementType.Insert, StatementType.Update, StatementType.Delete,
    };

    public static IReadOnlyCollection<AuditTrigger> CreateFromAuditTableAndEntityType<T>(AuditTable auditTable, IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var result = new List<AuditTrigger>();

        var tableName = entityType.GetTableName()!;
        var auditTableName = auditTable.Name;
        var auditEntityKeyColumnName = auditTable.Columns.Single(c => c.AuditedEntityKey).Name;
        var auditEntityKeyColumnType = auditTable.Columns.Single(c => c.AuditedEntityKey).Type;
        foreach (var triggerOperation in TriggerOperations)
        {
            var triggerName = GetTriggerName(options, tableName, auditTableName, auditEntityKeyColumnName, triggerOperation);
            result.Add(CreateAuditTrigger(triggerName, tableName, auditTableName, auditEntityKeyColumnName, auditEntityKeyColumnType, triggerOperation));
        }

        return result;
    }

    private static string GetTriggerName<T>(AuditOptions<T> options, string tableName, string auditTableName, string auditEntityKeyColumnName, StatementType triggerOperation) where T : class
    {
        var format = options.AuditTriggerNameFormat ?? DefaultTriggerNameFormat;
        var parameters = GetNameParameters(tableName, auditTableName, auditEntityKeyColumnName, triggerOperation);

        return Smart.Format(format, parameters);
    }

    private static AuditTriggerNameParameters GetNameParameters(string tableName, string auditTableName, string auditedEntityTableKeyColumnName, StatementType operationType)
        => new()
        {
            AuditPrefix = Constants.AuditTriggerPrefix,
            TableName = tableName,
            AuditTableName = auditTableName,
            AuditedEntityTableKeyColumnName = auditedEntityTableKeyColumnName,
            OperationType = operationType,
        };

    private static AuditTrigger CreateAuditTrigger(string name, string tableName, string auditTableName, string auditedEntityTableKeyColumnName, AuditColumnType auditedEntityTableKeyColumnType, StatementType operationType)
        => new(name, tableName, auditTableName, auditedEntityTableKeyColumnName, auditedEntityTableKeyColumnType, operationType);

    private class AuditTriggerNameParameters
    {
        public string? AuditPrefix { get; init; }

        public string? TableName { get; init; }

        public string? AuditTableName { get; init; }

        public string? AuditedEntityTableKeyColumnName { get; init; }

        public StatementType? OperationType { get; init; }
    }
}