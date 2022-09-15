using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.Extensions.DependencyInjection;
using CSharpMigrationOperationGenerator = EFCore.AuditExtensions.Common.Migrations.CSharp.CSharpMigrationOperationGenerator;
using CSharpMigrationsGenerator = EFCore.AuditExtensions.Common.Migrations.CSharp.CSharpMigrationsGenerator;

namespace EFCore.AuditExtensions.Common.EfCore;

public class DesignTimeServices : IDesignTimeServices
{
    public void ConfigureDesignTimeServices(IServiceCollection services)
    {
        services.AddSingleton<IMigrationsCodeGenerator, CSharpMigrationsGenerator>();
        services.AddSingleton<ICSharpMigrationOperationGenerator, CSharpMigrationOperationGenerator>();
    }
}