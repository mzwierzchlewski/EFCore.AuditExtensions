# EFCore Audit Extensions

The aim of this extension is an easy setup of auditing infrastructure for your entities. All changes (insert, update,
delete) to the entities are logged into a separate table (called _Audit Table_) with the following data:

* _Id_ - identifier of the modified entity
* _OldData_ - state of the entity before the change (serialized)
* _NewData_ - state of the entity after the change (serialized)
* _OperationType_ - the type of the change (insert, update, delete)
* _Timestamp_ - when the change happened
* _User_ - information on who made the change

All information is logged using database triggers on the original table. Everything needed for the extension to work is
created and later managed by it through EF Migrations.

## Installation (SQL Server)

1. Add `EFCore.AuditExtensions.SqlServer` to your project.
2. Add the following attribute to your project:

```csharp
[assembly: DesignTimeServicesReference("EFCore.AuditExtensions.Common.EfCore.DesignTimeServices, EFCore.AuditExtensions.Common")]
```

3. Use the `.UseSqlServerAudit()` extension of the `DbContextOptionsBuilder`, e.g.:

```csharp
var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices(
                   services =>
                   {
                       services.AddDbContext<ApplicationDbContext>(
                           options =>
                               options.UseSqlServer("<connection-string>")
                                      .UseSqlServerAudit());
                   }).Build();
```

4. Use the `IsAudited(...)` extension of the `EntityTypeBuilder` to select the entities which should be audited:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) 
        => modelBuilder.Entity<Product>().IsAudited();
}
```

5. Create a migration and update the database.

And that's all 🥳

## Configuration

The `IsAudited(...)` extension method allows some customizations through its `Action<AuditOptions<T>>? configureOptions`
parameter.

### Audit Table Name

By default, the _Audit Table_ is named using `<entity-table-name>_Audit`. This can be changed using
the `AuditOptions.AuditTableName` property:

```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTableName = "ProductAudit");
```

Given the `ApplicationDbContext` shown above, this will change the _Audit Table_'s name from `Products_Audit`
to `ProductAudit`.

### Audited Entity Key Selector

The audited entity **should** have a simple (composing of one property) primary key. By default, that's what the
extension will use. If no such key is found, it will default to another property. To select a specific property,
the `AuditedEntityKeySelector` option can be used:

```csharp
modelBuilder.Entity<Product>().IsAudited(
    options =>
    {
        options.AuditedEntityKeySelector = p => p.EAN;
    });
```

### Trigger Name Format

By default, the trigger name will be generated using the following pattern:

```
{AuditPrefix}_{TableName}_{AuditTableName}_{AuditedEntityTableKeyColumnName}_{OperationType}
```

This can be changed using the `AuditTriggerNameFormat` option:

```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTriggerNameFormat = "TRIGGER_{TableName}_{OperationType}");
```

The above configuration would change the trigger name from `Audit__Products_Products_Audit_ProductId_Insert`
to `TRIGGER_Products_Delete`.

### User Provider

By default, the _User_ column will be populated with the database user's name (with ` [db]` postfix, e.g. `sa [db]`). In
many cases that will not be enough. To provide more meaningful user information, implement the `IUserProvider`
interface:

```csharp
public class UserProvider : IUserProvider
{
    private readonly IHttpContextAccessor _httpContext;

    public UserProvider(IHttpContextAccessor httpContext)
    {
        _httpContext = httpContext;
    }

    public string GetCurrentUser() => _httpContext.HttpContext?.User.GetUserId() ?? "Anonymous";
}
```

And use the `UseSqlServerAudit<TUserProvider>()` extension of `DbContextOptionsBuilder`:

```csharp
var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices(
                   services =>
                   {
                       services.AddAuditUserProvider<UserProvider>();
                       services.AddDbContext<ApplicationDbContext>(
                           options =>
                               options.UseSqlServer("<connection-string>")
                                      .UseSqlServerAudit<UserProvider>());
                   }).Build();
```

## Compatibility

The extension is compatible with Entity Framework Core 6. Support for further EFCore release *may*<sup>[1](#guarantees)</sup> come.

Currently, only SQL Server database is supported. This will probably stay that way. To add support for other database
providers, use the `EFCore.AuditExtensions.SqlServer` project as your guide.

## Guarantees

No guarantees are provided - use at your own risk. 
