using EFCore.AuditableExtensions.Common.EfCoreExtension;
using EFCore.AuditableExtensions.Common.Interceptors;
using EFCore.AuditableExtensions.Common.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseAuditableExtension(
        this DbContextOptionsBuilder optionsBuilder,
        Action<IServiceCollection> addServices,
        Func<DbContextOptionsBuilder, DbContextOptionsBuilder> customiseDbContext,
        Func<IUserProvider, BaseUserContextInterceptor> createUserContextInterceptor,
        IServiceProvider serviceProvider)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new EfCoreAuditableExtension(InternalServiceCollectionExtensions.AddCommonServices + addServices));

        optionsBuilder.ReplaceService<IMigrationsModelDiffer, MigrationsModelDiffer>();
        customiseDbContext(optionsBuilder);

        return optionsBuilder.AddUserContextInterceptor(serviceProvider, createUserContextInterceptor);
    }

    private static DbContextOptionsBuilder AddUserContextInterceptor(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider, Func<IUserProvider, BaseUserContextInterceptor> createUserContextInterceptor)
    {
        var userProvider = serviceProvider.GetService<IUserProvider>();
        if (userProvider == null)
        {
            return optionsBuilder;
        }

        return optionsBuilder.AddInterceptors(createUserContextInterceptor(userProvider));
    }
}