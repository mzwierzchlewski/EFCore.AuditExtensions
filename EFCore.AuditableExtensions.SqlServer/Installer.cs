using EFCore.AuditableExtensions.Common.Extensions;
using EFCore.AuditableExtensions.Common.Interceptors;
using EFCore.AuditableExtensions.SqlServer.Interceptors;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.SqlServer;

public static class Installer
{
    public static IServiceCollection AddSqlServerAuditUserProvider<TUserProvider>(this IServiceCollection services) where TUserProvider : class, IUserProvider
    {
        services.AddScoped<IUserProvider, TUserProvider>();
        services.AddScoped<UserContextInterceptor>();

        return services;
    }

    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider) => optionsBuilder.UseAuditableExtension(AddSqlServerServices).CustomiseDbContext(serviceProvider);

    private static void AddSqlServerServices(this IServiceCollection services)
    {
        services.AddLogging();
        services.AddScoped<ICreateAuditTriggerSqlGenerator, CreateAuditTriggerSqlGenerator>();
        services.AddScoped<IDropAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator>();
    }

    private static DbContextOptionsBuilder CustomiseDbContext(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
    {
        optionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();

        var userContextInterceptor = serviceProvider.GetService<UserContextInterceptor>();
        if (userContextInterceptor == null)
        {
            return optionsBuilder;
        }

        return optionsBuilder.AddInterceptors(userContextInterceptor);
    }
}