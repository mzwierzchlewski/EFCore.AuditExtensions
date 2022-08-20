namespace EFCore.AuditableExtensions.Common.Migrations.CSharp.Operations;

public interface IDependentMigrationOperation
{
    Type[] DependsOn { get; }
}