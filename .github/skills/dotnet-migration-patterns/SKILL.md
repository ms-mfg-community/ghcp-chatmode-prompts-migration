---
name: dotnet-migration-patterns
description: >-
  Provides patterns and guidance for migrating .NET Framework applications to
  .NET 8+. Use when performing framework upgrades, converting WCF to REST,
  modernizing authentication, or transforming configuration files.
---

## Purpose

Provide concrete migration patterns for upgrading .NET Framework applications to modern .NET, covering the most common transformation scenarios.

## Framework Upgrade Paths

| Source | Target | Complexity |
|--------|--------|-----------|
| .NET Framework 4.x → .NET 8/10 | In-place upgrade | Medium |
| WinForms → .NET 8 WinForms | Desktop modernization | Low-Medium |
| WinForms → Blazor | Architecture redesign | High |
| WebForms → Blazor Server | Paradigm shift (stateful) | High |
| WebForms → Razor Pages | Traditional web | Medium |
| MVC 5 → ASP.NET Core MVC | Relatively direct | Medium |
| WCF → ASP.NET Core Web API | Service redesign | Medium-High |
| Web API 2 → ASP.NET Core | Near-direct port | Low |

## Configuration Migration

### web.config → appsettings.json
```xml
<!-- Before: web.config -->
<appSettings>
  <add key="ApiUrl" value="https://api.example.com" />
  <add key="MaxRetries" value="3" />
</appSettings>
<connectionStrings>
  <add name="DefaultConnection"
       connectionString="Server=.;Database=MyDb;Trusted_Connection=True;"
       providerName="System.Data.SqlClient" />
</connectionStrings>
```

```json
// After: appsettings.json
{
  "ApiUrl": "https://api.example.com",
  "MaxRetries": 3,
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyDb;Trusted_Connection=True;"
  }
}
```

### Configuration Access
```csharp
// Before
var value = ConfigurationManager.AppSettings["ApiUrl"];
var conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

// After
var value = configuration["ApiUrl"];
var conn = configuration.GetConnectionString("DefaultConnection");
// Or with strongly-typed options:
services.Configure<MyOptions>(configuration.GetSection("MyOptions"));
```

## WCF to REST Migration

### Service Contract → Controller
```csharp
// Before: WCF
[ServiceContract]
public interface IOrderService
{
    [OperationContract]
    Order GetOrder(int orderId);

    [OperationContract]
    void CreateOrder(Order order);
}

// After: ASP.NET Core Web API
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet("{orderId}")]
    public async Task<ActionResult<Order>> GetOrder(int orderId) { }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(Order order) { }
}
```

## Authentication Migration

### Windows/Forms Auth → Entra ID
```csharp
// Before: Forms Authentication
FormsAuthentication.SetAuthCookie(username, false);

// After: Microsoft Identity Web
services.AddMicrosoftIdentityWebAppAuthentication(configuration);
// Or for APIs:
services.AddMicrosoftIdentityWebApiAuthentication(configuration);
```

## Data Access Migration

### ADO.NET → Entity Framework Core
```csharp
// Before: ADO.NET
using (var conn = new SqlConnection(connStr))
{
    var cmd = new SqlCommand("SELECT * FROM Orders WHERE Id = @id", conn);
    cmd.Parameters.AddWithValue("@id", orderId);
    conn.Open();
    var reader = cmd.ExecuteReader();
}

// After: EF Core
var order = await context.Orders.FindAsync(orderId);
```

### Calling Stored Procedures from EF Core
```csharp
// When keeping SPs:
var results = await context.Orders
    .FromSqlInterpolated($"EXEC dbo.sp_GetOrdersByCustomer {customerId}")
    .ToListAsync();
```

## Dependency Injection

```csharp
// Before: Manual instantiation or Service Locator
var service = new OrderService(new OrderRepository());

// After: Built-in DI
services.AddScoped<IOrderService, OrderService>();
services.AddScoped<IOrderRepository, OrderRepository>();
```

## Key NuGet Package Replacements

| Old Package | New Package |
|------------|------------|
| `System.Data.SqlClient` | `Microsoft.Data.SqlClient` |
| `EntityFramework` (6.x) | `Microsoft.EntityFrameworkCore.SqlServer` |
| `Microsoft.AspNet.Mvc` | `Microsoft.AspNetCore.Mvc` (built-in) |
| `Microsoft.Owin` | ASP.NET Core middleware (built-in) |
| `Newtonsoft.Json` | `System.Text.Json` (or keep Newtonsoft) |
| `log4net` / `NLog` | `Microsoft.Extensions.Logging` + provider |
