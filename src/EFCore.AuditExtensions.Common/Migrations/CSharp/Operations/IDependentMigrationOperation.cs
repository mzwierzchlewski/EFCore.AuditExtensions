namespace EFCore.AuditExtensions.Common.Migrations.CSharp.Operations;

public interface IDependentMigrationOperation
{
    Type[] DependsOn { get; }
}