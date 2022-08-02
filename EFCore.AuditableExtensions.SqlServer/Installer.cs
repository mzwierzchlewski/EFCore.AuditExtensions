using EFCore.AuditableExtensions.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.SqlServer;

public static class Installer
{
    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseAuditableExtension(AddSqlServerServices);

    private static void AddSqlServerServices(this IServiceCollection services)
    {
    }
}