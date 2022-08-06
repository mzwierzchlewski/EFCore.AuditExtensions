namespace EFCore.AuditableExtensions.Common.Migrations.Operations;

public interface IDependentMigrationOperation
{
    Type[] DependsOn { get; }
}