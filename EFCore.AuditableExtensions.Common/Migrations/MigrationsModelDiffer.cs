using EFCore.AuditableExtensions.Common.EfCore;
using EFCore.AuditableExtensions.Common.Extensions;
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
        var auditMigrationOperations = new List<MigrationOperation>();

        var sourceAuditTables = GetEfCoreTablesFromAnnotations(source);
        var targetAuditTables = GetEfCoreTablesFromAnnotations(target);

        var diffContext = new DiffContext();
        auditMigrationOperations.AddRange(Diff(sourceAuditTables, targetAuditTables, diffContext));
        return base.GetDifferences(source, target).Concat(auditMigrationOperations).ToArray();
    }

    private IReadOnlyCollection<ITable> GetEfCoreTablesFromAnnotations(IRelationalModel? model)
    {
        var result = new List<ITable>();
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

            result.Add(audit.Table.ToEfCoreTable((RelationalModel)model!, TypeMappingSource));
        }

        return result.ToArray();
    }
}

#pragma warning restore EF1001