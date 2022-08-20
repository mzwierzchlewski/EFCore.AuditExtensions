using EFCore.AuditableExtensions.Common.EfCoreExtension;
using EFCore.AuditableExtensions.Common.Interceptors;
using EFCore.AuditableExtensions.Common.Migrations;
using EFCore.AuditableExtensions.Common.Migrations.Sql.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class DbContextOptionsBuilderExtensions
{
    internal static DbContextOptionsBuilder UseAuditableExtension<TUserProvider, TUserContextInterceptor, TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator, TMigrationsSqlGenerator>(
        this DbContextOptionsBuilder optionsBuilder) where TUserProvider : class, IUserProvider where TUserContextInterceptor : BaseUserContextInterceptor where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                new EfCoreAuditableExtension(
                    services =>
                    {
                        AddUserContextInterceptor<TUserProvider, TUserContextInterceptor>(services);
                        AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>(services);
                    }));

        return optionsBuilder.UseAuditableExtension<TMigrationsSqlGenerator>();
    }

    internal static DbContextOptionsBuilder UseAuditableExtension<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator, TMigrationsSqlGenerator>(
        this DbContextOptionsBuilder optionsBuilder) where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
            .AddOrUpdateExtension(
                new EfCoreAuditableExtension(
                    AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>));

        return optionsBuilder.UseAuditableExtension<TMigrationsSqlGenerator>();
    }

    private static DbContextOptionsBuilder UseAuditableExtension<TMigrationsSqlGenerator>(this DbContextOptionsBuilder optionsBuilder) where TMigrationsSqlGenerator : MigrationsSqlGenerator
    {
        optionsBuilder.ReplaceService<IMigrationsModelDiffer, MigrationsModelDiffer>();
        optionsBuilder.ReplaceService<IMigrationsSqlGenerator, TMigrationsSqlGenerator>();

        return optionsBuilder;
    }

    private static void AddUserContextInterceptor<TUserProvider, TUserContextInterceptor>(this IServiceCollection services) where TUserProvider : class, IUserProvider where TUserContextInterceptor : BaseUserContextInterceptor
    {
        services.AddScoped<IUserProvider, TUserProvider>();
        services.AddScoped<IInterceptor, TUserContextInterceptor>();
    }

    private static void AddCommonServices<TCreateAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>(this IServiceCollection services) where TCreateAuditTriggerSqlGenerator : class, ICreateAuditTriggerSqlGenerator where TDropAuditTriggerSqlGenerator : class, IDropAuditTriggerSqlGenerator
    {
        services.AddLogging();
        services.AddScoped<ICreateAuditTriggerSqlGenerator, TCreateAuditTriggerSqlGenerator>();
        services.AddScoped<IDropAuditTriggerSqlGenerator, TDropAuditTriggerSqlGenerator>();
    }
}