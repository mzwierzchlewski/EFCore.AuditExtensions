using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartFormat;

namespace EFCore.AuditExtensions.Common.Annotations.Trigger;

internal static class AuditTriggerFactory
{
    private const string DefaultTriggerNameFormat = "{AuditPrefix}_{TableName}_{AuditTableName}";

    public static AuditTrigger CreateFromAuditTableAndEntityType<T>(AuditTable auditTable, IReadOnlyEntityType entityType, AuditOptions<T> options) where T : class
    {
        var tableName = entityType.GetTableName()!;
        var auditTableName = auditTable.Name;
        var auditEntityKeyColumnName = auditTable.Columns.Single(c => c.AuditedEntityKey).Name;
        var auditEntityKeyColumnType = auditTable.Columns.Single(c => c.AuditedEntityKey).Type;
        var updateOptimisationThreshold = GetUpdateOptimisationThreshold(options.AuditTriggerOptions);
        var noKeyChanges = GetNoKeyChanges(options.AuditTriggerOptions);

        var triggerName = GetTriggerName(options.AuditTriggerOptions, tableName, auditTableName);
        return CreateAuditTrigger(triggerName, tableName, auditTableName, auditEntityKeyColumnName, auditEntityKeyColumnType, updateOptimisationThreshold, noKeyChanges);
    }

    private static bool GetNoKeyChanges<T>(AuditTriggerOptions<T> options) where T : class => options.NoKeyChanges ?? false;

    private static int GetUpdateOptimisationThreshold<T>(AuditTriggerOptions<T> options) where T : class => options.UpdateOptimisationThreshold ?? 100;

    private static string GetTriggerName<T>(AuditTriggerOptions<T> options, string tableName, string auditTableName) where T : class
    {
        var format = options.NameFormat ?? DefaultTriggerNameFormat;
        var parameters = GetNameParameters(tableName, auditTableName);

        return Smart.Format(format, parameters);
    }

    private static AuditTriggerNameParameters GetNameParameters(string tableName, string auditTableName)
        => new()
        {
            AuditPrefix = Constants.AuditTriggerPrefix,
            TableName = tableName,
            AuditTableName = auditTableName,
        };

    private static AuditTrigger CreateAuditTrigger(string name, string tableName, string auditTableName, string auditedEntityTableKeyColumnName, AuditColumnType auditedEntityTableKeyColumnType, int updateOptimisationThreshold, bool noKeyChanges)
        => new(name, tableName, auditTableName, auditedEntityTableKeyColumnName, auditedEntityTableKeyColumnType, updateOptimisationThreshold, noKeyChanges);

    private class AuditTriggerNameParameters
    {
        public string? AuditPrefix { get; init; }

        public string? TableName { get; init; }

        public string? AuditTableName { get; init; }
    }
}