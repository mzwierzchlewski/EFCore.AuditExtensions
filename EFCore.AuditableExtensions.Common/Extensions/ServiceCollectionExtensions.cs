using EFCore.AuditableExtensions.Common.Interceptors;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.Extensions;

public static class PublicServiceCollectionExtensions
{
    public static IServiceCollection AddAuditUserProvider<TUserProvider>(this IServiceCollection services) where TUserProvider : class, IUserProvider => services.AddScoped<IUserProvider, TUserProvider>();
}

internal static class InternalServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services) => services.AddLogging();
}