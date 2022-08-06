using System.Data;
using EFCore.AuditableExtensions.Common.Annotations.Table;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFCore.AuditableExtensions.Common.Annotations.Trigger;

internal static class AuditTriggerFactory
{
    private static readonly StatementType[] TriggerOperations =
    {
        StatementType.Insert, StatementType.Update, StatementType.Delete,
    };

    public static IReadOnlyCollection<AuditTrigger> CreateFromAuditTableAndEntityType(AuditTable auditTable, IReadOnlyEntityType entityType)
    {
        var result = new List<AuditTrigger>();

        var tableName = entityType.GetTableName()!;
        var auditTableName = auditTable.Name;
        var auditEntityKeyColumnName = auditTable.Columns.Single(c => c.AuditedEntityKey).Name;
        foreach (var triggerOperation in TriggerOperations)
        {
            var triggerName = GenerateTriggerName(tableName, auditTableName, auditEntityKeyColumnName, triggerOperation);
            result.Add(CreateAuditTrigger(triggerName, tableName, auditTableName, auditEntityKeyColumnName, triggerOperation));
        }

        return result;
    }

    private static string GenerateTriggerName(string tableName, string auditTableName, string auditedEntityTableKeyColumnName, StatementType operationType)
        => $"{Constants.AuditTriggerPrefix}_{tableName}_{auditTableName}_{auditTableName}_{auditedEntityTableKeyColumnName}_{operationType.ToString()}";

    private static AuditTrigger CreateAuditTrigger(string name, string tableName, string auditTableName, string auditedEntityTableKeyColumnName, StatementType operationType)
        => new(name, tableName, auditTableName, auditedEntityTableKeyColumnName, operationType);
}