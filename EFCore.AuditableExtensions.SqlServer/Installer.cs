using EFCore.AuditableExtensions.Common.Extensions;
using EFCore.AuditableExtensions.Common.Interceptors;
using EFCore.AuditableExtensions.SqlServer.Interceptors;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators;
using EFCore.AuditableExtensions.SqlServer.SqlGenerators.Operations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditableExtensions.SqlServer;

public static class Installer
{
    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseAuditableExtension<CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
    
    public static DbContextOptionsBuilder UseSqlServerAudit<TUserProvider>(this DbContextOptionsBuilder optionsBuilder) where TUserProvider : class, IUserProvider
        => optionsBuilder.UseAuditableExtension<TUserProvider, UserContextInterceptor, CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
}