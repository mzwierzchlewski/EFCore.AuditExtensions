using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.EfCoreExtension;

public class EfCoreAuditableExtension : IDbContextOptionsExtension
{
    private readonly Action<IServiceCollection> _addServices;

    public DbContextOptionsExtensionInfo Info { get; }

    public EfCoreAuditableExtension(Action<IServiceCollection> addServices)
    {
        _addServices = addServices;
        Info = new EfCoreAuditableExtensionInfo(this);
    }

    public void ApplyServices(IServiceCollection services) => _addServices(services);

    public void Validate(IDbContextOptions options)
    { }
}