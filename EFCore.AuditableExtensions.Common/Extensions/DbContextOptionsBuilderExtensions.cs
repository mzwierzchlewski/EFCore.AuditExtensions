using System.Runtime.CompilerServices;
using EFCore.AuditableExtensions.Common.EfCoreExtension;
using EFCore.AuditableExtensions.Common.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("EFCore.AuditableExtensions.SqlServer")]

namespace EFCore.AuditableExtensions.Common.Extensions;

internal static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseAuditableExtension(
        this DbContextOptionsBuilder optionsBuilder,
        Action<IServiceCollection> addServices)
    {
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(new EfCoreAuditableExtension(addServices));

        return optionsBuilder.ReplaceService<IMigrationsModelDiffer, MigrationsModelDiffer>();
    }
}