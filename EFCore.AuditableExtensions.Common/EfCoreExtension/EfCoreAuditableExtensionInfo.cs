using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.AuditableExtensions.Common.EfCoreExtension;

public class EfCoreAuditableExtensionInfo : DbContextOptionsExtensionInfo
{
    public override bool IsDatabaseProvider => false;

    public override string LogFragment => "EfCoreAuditableExtension";

    public EfCoreAuditableExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    { }

    public override int GetServiceProviderHashCode() => 0;

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => string.Equals(LogFragment, other.LogFragment, StringComparison.Ordinal);

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    { }
}