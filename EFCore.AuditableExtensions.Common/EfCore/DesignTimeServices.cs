using EFCore.AuditableExtensions.Common.Migrations;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;

namespace EFCore.AuditableExtensions.Common.EfCore;

public class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
        => services.AddSingleton<ICSharpMigrationOperationGenerator, MigrationsCSharpGenerator>();
}