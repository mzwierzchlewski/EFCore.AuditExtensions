# EFCore Audit Extensions

The aim of this extension is an easy setup of auditing infrastructure for your entities. All changes (insert, update,
delete) to the entities are logged into a separate tables (called _Audit Table_), one audit table per EF entity, with the following data:

* _Id_ - identifier of the modified entity <sup>[1](#id-column)</sup>
* _OldData_ - state of the entity before the change (serialized)
* _NewData_ - state of the entity after the change (serialized)
* _OperationType_ - the type of the change (insert, update, delete)
* _Timestamp_ - when the change happened
* _User_ - information on who made the change

<a name="id-column">1</a> If the identifier (key) is made of multiple columns, all of these columns will we replicated in the Audit Table.

All information is logged using a database trigger on the original table. Everything needed for the extension to work is
created and later managed by it through EF Migrations.

## Installation (SQL Server)

1. Add `EFCore.AuditExtensions.SqlServer` reference to your project.
2. Add the following attribute to your **startup/data** project:

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

The `IsAudited(...)` extension method allows some customisations through its `Action<AuditOptions<T>>? configureOptions`
parameter.

### Audit Table Name

By default, the _Audit Table_ is named using `<entity-table-name>_Audit`. This can be changed using
the `AuditOptions.AuditTableName` property:

```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTableName = "ProductAudit");
```

Given the `ApplicationDbContext` shown above, this will change the _Audit Table_'s name from `Products_Audit`
to `ProductAudit`.

### Audited Entity Key Options

The audited entity **must** have a key. By default, that's what the extension will use. 
Primary keys are given priority over other keys. To select specific properties,
the `AuditedEntityKeyOptions.KeySelector` option can be used:

```csharp
modelBuilder.Entity<Product>().IsAudited(
    options =>
    {
        options.AuditedEntityKeyOptions.KeySelector = p => new { p.EAN };
    });
```

There are two additional options regarding the Audited Entity Key columns:
* `AuditedEntityKeyOptions.Index` - if `true`, an index will be created on the column. This defaults to `true` when no `KeySelector` is specified, and to `false` when it is.
* `AuditedEntityKeyOptions.IndexName` - name for the index. Defaults to default Entity Framework index name convention (`IX_{TableName}_{Column1Name}_{Column2Name}`).


### Audit Trigger Options
#### Name Format

By default, the trigger name will be generated using the following pattern:

```
{AuditPrefix}_{TableName}_{AuditTableName}
```

This can be changed using the `AuditTriggerNameFormat` option:

```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTriggerOptions.NameFormat = "TRIGGER_{TableName}");
```

The above configuration would change the trigger name from `Audit__Products_Products_Auditt`
to `TRIGGER_Products`.

#### UPDATE Optimization Threshold (SQL Server)

In SQL Server the `Inserted` and `Deleted` tables do not have any indexes on them which makes any joins between them really painful.
This can be helped by using table variables with indexes on the key columns. As that comes with a performance overhead of its own,
this setting decides the minimum number of updated rows for which the table variable approach will be used. Defaults to `100`.
```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTriggerOptions.UpdateOptimisationThreshold = 500);
```

#### No Key Changes

To prevent any data loss when logging entity updates a `FULL OUTER JOIN` is made between the `Inserted` and `Deleted` tables.
If it is guaranteed that the entity's keys will never change, the `AuditTriggerOptions.NoKeyChanges` property can be set to `true`.
This will result in `INNER JOIN` being used as well as fewer `COALESCE()` calls. Defaults to `false`.

```csharp
modelBuilder.Entity<Product>().IsAudited(options => options.AuditTriggerOptions.NoKeyChanges = true);
```

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

The extension is compatible with Entity Framework Core 6 ([main branch](https://github.com/mzwierzchlewski/EFCore.AuditExtensions/tree/main)) and Entity Framework Core 7 ([ef7 branch](https://github.com/mzwierzchlewski/EFCore.AuditExtensions/tree/ef7)).

Currently, only SQL Server database is supported. This will probably stay that way. To add support for other database
providers, use [this blog post](https://maciejz.dev/ef-core-audit-extensions/#adding-support-for-other-database-engines) and the `EFCore.AuditExtensions.SqlServer` project as your guide.

## Sample trigger SQL
```sql
CREATE TRIGGER [dbo].[Audit__Products_Products_Audit] ON [dbo].[Products]
FOR INSERT, UPDATE, DELETE AS
BEGIN
    IF @@ROWCOUNT = 0 RETURN;
    SET NOCOUNT ON;
    DECLARE @user varchar(255);
    SET @user = COALESCE(CAST(SESSION_CONTEXT(N'user') AS VARCHAR(255)), CONCAT(SUSER_NAME(), ' [db]'));

    -- Handle UPDATE statements
    IF EXISTS(SELECT * FROM Inserted) AND EXISTS(SELECT * FROM Deleted)
    BEGIN
        IF @@ROWCOUNT < 100
        BEGIN
            INSERT INTO [Products_Audit] ([ProductId], [OldData], [NewData], [OperationType], [User], [Timestamp])
            SELECT COALESCE(D.[ProductId], I.[ProductId]), (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [OldData], (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [NewData], 'UPDATE', @user, GETUTCDATE()
            FROM Deleted D FULL OUTER JOIN Inserted I ON D.[ProductId] = I.[ProductId];
        END;
        ELSE
        BEGIN
            -- Create table variables with inserted and deleted data
            -- and indexes on the key columns that will help with joins
            DECLARE @Products_Audit_Deleted TABLE (
                [ProductId] int,
                [OldData] NVARCHAR(MAX)
                PRIMARY KEY CLUSTERED ([ProductId]));
            INSERT INTO @Products_Audit_Deleted
            SELECT [ProductId], (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [OldData]
            FROM Deleted D;

            DECLARE @Products_Audit_Inserted TABLE  (
                [ProductId] int,
                [NewData] NVARCHAR(MAX)
                PRIMARY KEY CLUSTERED ([ProductId]));
            INSERT INTO @Products_Audit_Inserted
            SELECT [ProductId], (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS [NewData]
            FROM Inserted I;

            -- COALESCE and FULL OUTER JOIN prevent loss of data when value of the primary key was changed
            INSERT INTO [Products_Audit] ([ProductId], [OldData], [NewData], [OperationType], [User], [Timestamp])
            SELECT COALESCE(D.[ProductId], I.[ProductId]), D.[OldData], I.[NewData], 'UPDATE', @user, GETUTCDATE()
            FROM @Products_Audit_Deleted D FULL OUTER JOIN @Products_Audit_Inserted I ON D.[ProductId] = I.[ProductId];
        END;
    END;
    -- Handle INSERT statements
    ELSE IF EXISTS(SELECT * FROM Inserted)
    BEGIN
        INSERT INTO [Products_Audit] ([ProductId], [OldData], [NewData], [OperationType], [User], [Timestamp])
        SELECT [ProductId], NULL, (SELECT I.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), 'INSERT', @user, GETUTCDATE()
        FROM Inserted I;
    END;
    -- Handle DELETE statements
    ELSE IF EXISTS(SELECT * FROM Deleted)
    BEGIN
        INSERT INTO [Products_Audit] ([ProductId], [OldData], [NewData], [OperationType], [User], [Timestamp])
        SELECT [ProductId], (SELECT D.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER), NULL, 'DELETE', @user, GETUTCDATE()
        FROM Deleted D;
    END;
END;
```

## Guarantees

No guarantees are provided - use at your own risk. 
