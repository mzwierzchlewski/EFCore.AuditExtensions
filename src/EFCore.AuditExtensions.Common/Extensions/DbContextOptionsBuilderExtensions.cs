using EFCore.AuditExtensions.Common.EfCoreExtension;
using EFCore.AuditExtensions.Common.Interceptors;
using EFCore.AuditExtensions.Common.Migrations;
using EFCore.AuditExtensions.Common.Migrations.Sql.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ModelCustomizer = EFCore.AuditExtensions.Common.EfCore.ModelCustomizer;

namespace EFCore.AuditExtensions.Common.Extensions;

internal static class DbContextOptionsBuilderExtensions
{
    internal static DbContextOptionsBuilder UseAuditExtension<TUserProvider, TUserContextInterceptor, TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator, TMigrationsSqlGenerator>(this DbContextOptionsBuilder optionsBuilder)
        where TUserProvider : class, IUserProvider where TUserContextInterceptor : BaseUserContextInterceptor where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                new EfCoreAuditExtension(
                    services =>
                    {
                        AddUserContextInterceptor<TUserProvider, TUserContextInterceptor>(services);
                        AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>(services);
                    }));

        return optionsBuilder.UseAuditExtension<TMigrationsSqlGenerator>();
    }

    internal static DbContextOptionsBuilder UseAuditExtension<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator, TMigrationsSqlGenerator>(this DbContextOptionsBuilder optionsBuilder)
        where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                new EfCoreAuditExtension(
                    AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>));

        return optionsBuilder.UseAuditExtension<TMigrationsSqlGenerator>();
    }

    private static DbContextOptionsBuilder UseAuditExtension<TMigrationsSqlGenerator>(this DbContextOptionsBuilder optionsBuilder) where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        optionsBuilder.ReplaceService<IMigrationsModelDiffer, MigrationsModelDiffer>();
        optionsBuilder.ReplaceService<IMigrationsSqlGenerator, TMigrationsSqlGenerator>();

        return optionsBuilder;
    }

    private static void AddUserContextInterceptor<TUserProvider, TUserContextInterceptor>(this IServiceCollection services) where TUserProvider : class, IUserProvider where TUserContextInterceptor : BaseUserContextInterceptor
    {
        services.AddScoped<IUserProvider>(
            provider =>
            {
                var applicationServiceProvider = provider
                                                 .GetService<IDbContextOptions>()?
                                                 .FindExtension<CoreOptionsExtension>()?
                                                 .ApplicationServiceProvider;
                if (applicationServiceProvider == null)
                {
                    return new EmptyUserProvider();
                }

                var userProvider = ActivatorUtilities.GetServiceOrCreateInstance<TUserProvider>(applicationServiceProvider);
                return userProvider;
            });
        services.AddScoped<IInterceptor, TUserContextInterceptor>();
    }

    private static void AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>(this IServiceCollection services) where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator
    {
        services.AddLogging();
        services.AddScoped<ICreateAuditTriggerSqlGenerator, TCreateAuditTriggerSqlGenerator>();
        services.AddScoped<IDropAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>();
        services.AddSingleton<IModelCustomizer, ModelCustomizer>();
    }
}