using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditExtensions.Common.EfCoreExtension;

public class EfCoreAuditExtension : IDbContextOptionsExtension
{
    private readonly Action<IServiceCollection> _addServices;

    public DbContextOptionsExtensionInfo Info { get; }

    public EfCoreAuditExtension(Action<IServiceCollection> addServices)
    {
        _addServices = addServices;
        Info = new EfCoreAuditExtensionInfo(this);
    }

    public void ApplyServices(IServiceCollection services) => _addServices(services);

    public void Validate(IDbContextOptions options)
    { }
}