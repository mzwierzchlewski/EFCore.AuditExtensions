using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using CSharpMigrationOperationGenerator = EFCore.AuditableExtensions.Common.Migrations.CSharpMigrationOperationGenerator;
using CSharpMigrationsGenerator = EFCore.AuditableExtensions.Common.Migrations.CSharpMigrationsGenerator;

namespace EFCore.AuditableExtensions.Common.EfCore;

public class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>();
        services.AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>();
    }
}