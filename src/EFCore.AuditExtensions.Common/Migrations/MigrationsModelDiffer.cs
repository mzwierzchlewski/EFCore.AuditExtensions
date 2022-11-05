using EFCore.AuditExtensions.Common.Annotations;
using EFCore.AuditExtensions.Common.Annotations.Trigger;
using EFCore.AuditExtensions.Common.EfCore;
using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;
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

namespace EFCore.AuditExtensions.Common.Migrations;

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
        var dropIndexOperations = auditMigrationOperationsArray.OfType<DropIndexOperation>().ToArray();
        var firstOperations = dropAuditTriggerOperations.Concat<MigrationOperation>(dropIndexOperations).ToArray();
        var leftoverAuditOperations = auditMigrationOperationsArray.Except(firstOperations);

        return firstOperations.Concat(otherOperations).Concat(leftoverAuditOperations).ToList();
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

        var triggerOperations = Diff(source.Audit.Trigger, target.Audit.Trigger, diffContext);
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
        
        foreach (var operation in Add(target.Audit.Trigger, diffContext))
        {
            yield return operation;
        }
    }

    private IEnumerable<MigrationOperation> Remove(AuditedEntityType source, DiffContext diffContext)
    {
        foreach (var operation in Remove(source.Audit.Trigger, diffContext))
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

    private static IEnumerable<MigrationOperation> Diff(AuditTrigger source, AuditTrigger target, DiffContext diffContext)
    {
        if (source == target)
        {
            yield break;
        }

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
            target.KeyProperties,
            target.UpdateOptimisationThreshold,
            target.NoKeyChanges);
    }

    private static IEnumerable<MigrationOperation> Remove(AuditTrigger source, DiffContext diffContext)
    {
        yield return new DropAuditTriggerOperation(source.Name);
    }

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