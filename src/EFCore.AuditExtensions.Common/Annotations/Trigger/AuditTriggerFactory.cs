using EFCore.AuditExtensions.Common.Annotations.Table;
using EFCore.AuditExtensions.Common.Configuration;
using EFCore.AuditExtensions.Common.SharedModels;
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
        var auditEntityKeyProperties = GetKeyProperties(auditTable);
        var updateOptimisationThreshold = GetUpdateOptimisationThreshold(options.AuditTriggerOptions);
        var noKeyChanges = GetNoKeyChanges(options.AuditTriggerOptions);

        var triggerName = GetTriggerName(options.AuditTriggerOptions, tableName, auditTableName);
        return new AuditTrigger(triggerName, tableName, auditTableName, auditEntityKeyProperties, updateOptimisationThreshold, noKeyChanges);
    }

    private static AuditedEntityKeyProperty[] GetKeyProperties(AuditTable auditTable) 
        => auditTable.Columns.Where(c => c.AuditedEntityKey).Select(c => new AuditedEntityKeyProperty(c.Name, c.Type, c.MaxLength)).ToArray();

    private static bool GetNoKeyChanges(AuditTriggerOptions options) => options.NoKeyChanges ?? false;

    private static int GetUpdateOptimisationThreshold(AuditTriggerOptions options) => options.UpdateOptimisationThreshold ?? 100;

    private static string GetTriggerName(AuditTriggerOptions options, string tableName, string auditTableName)
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

    private class AuditTriggerNameParameters
    {
        public string? AuditPrefix { get; init; }

        public string? TableName { get; init; }

        public string? AuditTableName { get; init; }
    }
}