using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection services) => services.AddLogging();
}