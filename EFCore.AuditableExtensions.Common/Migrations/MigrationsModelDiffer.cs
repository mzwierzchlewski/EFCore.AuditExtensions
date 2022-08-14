using EFCore.AuditableExtensions.Common.Annotations;
using EFCore.AuditableExtensions.Common.Annotations.Trigger;
using EFCore.AuditableExtensions.Common.EfCore;
using EFCore.AuditableExtensions.Common.Extensions;
using EFCore.AuditableExtensions.Common.Migrations.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Logging;

namespace EFCore.AuditableExtensions.Common.Migrations;

#pragma warning disable EF1001

public class MigrationsModelDiffer : Microsoft.EntityFrameworkCore.Migrations.Internal.MigrationsModelDiffer
{
    private readonly ILogger<MigrationsModelDiffer> _logger;

    public MigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource,
        IMigrationsAnnotationProvider migrationsAnnotations,
        IChangeDetector changeDetector,
        IUpdateAdapterFactory updateAdapterFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies,
        ILogger<MigrationsModelDiffer> logger)
        : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
    {
        _logger = logger;
    }

    public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
    {
        var sourceAuditedEntityTypes = GetAuditedEntityTypes(source);
        var targetAuditedEntityTypes = GetAuditedEntityTypes(target);

        var diffContext = new DiffContext();
        var auditMigrationOperations = Diff(sourceAuditedEntityTypes, targetAuditedEntityTypes, diffContext);
        var baseMigrationOperations = base.GetDifferences(source, target);

        return Sort(auditMigrationOperations, baseMigrationOperations);
    }

    private static IReadOnlyList<MigrationOperation> Sort(IEnumerable<MigrationOperation> auditMigrationOperations, IEnumerable<MigrationOperation> otherOperations)
    {
        var auditMigrationOperationsArray = auditMigrationOperations as MigrationOperation[] ?? auditMigrationOperations.ToArray();
        var dropAuditTriggerOperations = auditMigrationOperationsArray.OfType<DropAuditTriggerOperation>().ToArray();
        var leftoverAuditOperations = auditMigrationOperationsArray.Except(dropAuditTriggerOperations);

        return dropAuditTriggerOperations.Concat(otherOperations).Concat(leftoverAuditOperations).ToList();
    }

    #region Diff - AuditedEntityType

    private IEnumerable<MigrationOperation> Diff(
        IEnumerable<AuditedEntityType> source,
        IEnumerable<AuditedEntityType> target,
        DiffContext diffContext)
        => DiffCollection(source, target, diffContext, Diff, Add, Remove, CompareAuditedEntityTypes);

    private IEnumerable<MigrationOperation> Diff(AuditedEntityType source, AuditedEntityType target, DiffContext diffContext)
    {
        var sourceTable = source.Audit.Table.ToEfCoreTable((RelationalModel)source.EntityType.Model.GetRelationalModel(), TypeMappingSource);
        var targetTable = target.Audit.Table.ToEfCoreTable((RelationalModel)target.EntityType.Model.GetRelationalModel(), TypeMappingSource);
        var tableOperations = Diff(sourceTable, targetTable, diffContext);
        foreach (var operation in tableOperations)
        {
            yield return operation;
        }

        var triggerOperations = Diff(source.Audit.Triggers, target.Audit.Triggers, diffContext);
        foreach (var operation in triggerOperations)
        {
            yield return operation;
        }
    }

    private IEnumerable<MigrationOperation> Add(AuditedEntityType target, DiffContext diffContext)
    {
        var targetTable = target.Audit.Table.ToEfCoreTable((RelationalModel)target.EntityType.Model.GetRelationalModel(), TypeMappingSource);
        foreach (var operation in Add(targetTable, diffContext))
        {
            yield return operation;
        }

        foreach (var trigger in target.Audit.Triggers)
        foreach (var operation in Add(trigger, diffContext))
        {
            yield return operation;
        }
    }

    private IEnumerable<MigrationOperation> Remove(AuditedEntityType source, DiffContext diffContext)
    {
        foreach (var trigger in source.Audit.Triggers)
        foreach (var operation in Remove(trigger, diffContext))
        {
            yield return operation;
        }

        var targetTable = source.Audit.Table.ToEfCoreTable((RelationalModel)source.EntityType.Model.GetRelationalModel(), TypeMappingSource);
        foreach (var operation in Remove(targetTable, diffContext))
        {
            yield return operation;
        }
    }

    private static bool CompareAuditedEntityTypes(AuditedEntityType source, AuditedEntityType target, DiffContext diffContext) => source.EntityType.Name == target.EntityType.Name;

    #endregion

    #region Diff - AuditTrigger

    private IEnumerable<MigrationOperation> Diff(
        IEnumerable<AuditTrigger> source,
        IEnumerable<AuditTrigger> target,
        DiffContext diffContext)
        => DiffCollection(source, target, diffContext, Diff, Add, Remove, CompareAuditTriggers);

    private static IEnumerable<MigrationOperation> Diff(AuditTrigger source, AuditTrigger target, DiffContext diffContext)
    {
        var dropOperations = Remove(source, diffContext);
        foreach (var operation in dropOperations)
        {
            yield return operation;
        }

        var addOperations = Add(target, diffContext);
        foreach (var operation in addOperations)
        {
            yield return operation;
        }
    }

    private static IEnumerable<MigrationOperation> Add(AuditTrigger target, DiffContext diffContext)
    {
        yield return new CreateAuditTriggerOperation(
            target.TableName,
            target.AuditTableName,
            target.Name,
            target.OperationType,
            target.AuditedEntityTableKeyColumnName,
            target.AuditedEntityTableKeyColumnType);
    }

    private static IEnumerable<MigrationOperation> Remove(AuditTrigger source, DiffContext diffContext)
    {
        yield return new DropAuditTriggerOperation(source.Name);
    }

    private static bool CompareAuditTriggers(AuditTrigger source, AuditTrigger target, DiffContext diffContext) => source == target;

    #endregion

    #region AuditedEntityType

    private IReadOnlyCollection<AuditedEntityType> GetAuditedEntityTypes(IRelationalModel? model)
    {
        var result = new List<AuditedEntityType>();
        var entityTypes = model?.Model.GetAuditedEntityTypes() ?? Array.Empty<IEntityType>();
        foreach (var entityType in entityTypes)
        {
            var auditAnnotation = entityType.GetAuditAnnotation();
            if (auditAnnotation.Value is not string serializedAudit)
            {
                _logger.LogWarning("Invalid Audit Annotation value for Entity Type: {EntityTypeName}", entityType.Name);
                continue;
            }

            var audit = serializedAudit.Deserialize();
            if (audit == null)
            {
                _logger.LogWarning("Invalid serialized Audit in Audit Annotation for Entity Type {EntityTypeName}: {SerializedAudit}", entityType.Name, serializedAudit);
                continue;
            }

            result.Add(new AuditedEntityType(entityType, audit));
        }

        return result.ToArray();
    }

    private class AuditedEntityType
    {
        public IEntityType EntityType { get; }

        public Audit Audit { get; }

        public AuditedEntityType(IEntityType entityType, Audit audit)
        {
            EntityType = entityType;
            Audit = audit;
        }
    }

    #endregion
}

#pragma warning restore EF1001