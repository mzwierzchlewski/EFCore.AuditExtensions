using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.Interceptors;
using EFCore.AuditExtensions.SqlServer.Interceptors;
using EFCore.AuditExtensions.SqlServer.SqlGenerators;
using EFCore.AuditExtensions.SqlServer.SqlGenerators.Operations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditExtensions.SqlServer;

public static class Installer
{
    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseAuditExtension<CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
    
    public static DbContextOptionsBuilder UseSqlServerAudit<TUserProvider>(this DbContextOptionsBuilder optionsBuilder) where TUserProvider : class, IUserProvider
        => optionsBuilder.UseAuditExtension<TUserProvider, UserContextInterceptor, CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
}