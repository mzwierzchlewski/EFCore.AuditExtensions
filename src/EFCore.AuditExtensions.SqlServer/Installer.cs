using EFCore.AuditExtensions.Common.Extensions;
using EFCore.AuditExtensions.Common.Interceptors;
using EFCore.AuditExtensions.SqlServer.Interceptors;
using EFCore.AuditExtensions.SqlServer.SqlGenerators;
using EFCore.AuditExtensions.SqlServer.SqlGenerators.Operations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.AuditExtensions.SqlServer;

public static class Installer
{
    /// <summary>
    /// Adds Audit Extensions to the DbContext.
    /// If a custom <code>IUserProvider</code> implementation is available use <see cref="UseSqlServerAudit&lt;TUserProvider&gt;"/>.
    /// </summary>
    /// <param name="optionsBuilder">DbContextOptionsBuilder for the DBContext.</param>
    /// <returns>DbContextOptionsBuilder for further chaining.</returns>
    public static DbContextOptionsBuilder UseSqlServerAudit(this DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseAuditExtension<CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
    
    /// <summary>
    /// Adds Audit Extensions to the DbContext with a custom <code>IUserProvider</code> implementation.
    /// </summary>
    /// <param name="optionsBuilder">DbContextOptionsBuilder for the DBContext.</param>
    /// <typeparam name="TUserProvider">Implementation of <code>IUserProvider</code></typeparam>
    /// <returns>DbContextOptionsBuilder for further chaining.</returns>
    public static DbContextOptionsBuilder UseSqlServerAudit<TUserProvider>(this DbContextOptionsBuilder optionsBuilder) where TUserProvider : class, IUserProvider
        => optionsBuilder.UseAuditExtension<TUserProvider, UserContextInterceptor, CreateAuditTriggerSqlGenerator, DropAuditTriggerSqlGenerator, SqlServerMigrationsSqlGenerator>();
}