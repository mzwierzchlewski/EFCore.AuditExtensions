using EFCore.AuditableExtensions.Common.Extensions;
using EFCore.AuditableExtensions.Common.Interceptors;
using EFCore.AuditableExtensions.SqlServer.Interceptors;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using MigrationsSqlGenerator = EFCore.AuditableExtensions.SqlServer.SqlGenerators.MigrationsSqlGenerator;

namespace EFCore.AuditableExtensions.SqlServer;

public static class Installer
{
    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
        => optionsBuilder.UseAuditableExtension(AddSqlServerServices, CustomiseSqlServerDbContext, CreateUserContextInterceptor, serviceProvider);

    private static void AddSqlServerServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateAuditTriggerSqlGenerator, CreateAuditTriggerSqlGenerator>();
        services.AddScoped<IDropAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator>();
    }

    private static BaseUserContextInterceptor CreateUserContextInterceptor(IUserProvider userProvider) => new UserContextInterceptor(userProvider);

    private static DbContextOptionsBuilder CustomiseSqlServerDbContext(this DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.ReplaceService<IMigrationsSqlGenerator, MigrationsSqlGenerator>();
}