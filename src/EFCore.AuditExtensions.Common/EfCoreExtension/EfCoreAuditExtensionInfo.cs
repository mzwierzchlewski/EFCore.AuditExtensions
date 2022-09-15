using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.AuditExtensions.Common.EfCoreExtension;

public class EfCoreAuditExtensionInfo : DbContextOptionsExtensionInfo
{
    public override bool IsDatabaseProvider => false;

    public override string LogFragment => "EfCoreAuditExtension";

    public EfCoreAuditExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    { }

    public override int GetServiceProviderHashCode() => 0;

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => string.Equals(LogFragment, other.LogFragment, StringComparison.Ordinal);

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    { }
}