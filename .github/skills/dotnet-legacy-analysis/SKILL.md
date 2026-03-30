---
name: dotnet-legacy-analysis
description: >-
  Analyzes legacy .NET Framework applications (WinForms, WebForms, MVC, WCF) for
  migration readiness. Use when analyzing C# code structure, identifying framework-specific
  patterns, extracting business logic, or understanding legacy .NET architectures.
---

## Purpose

Provide specialized knowledge for analyzing legacy .NET Framework applications across all UI technologies (WinForms, WebForms, MVC, WCF) to support migration planning and business logic extraction.

## Application Type Detection

Identify the application type by looking for these markers:

| Type | Key Indicators |
|------|---------------|
| **WinForms** | `InitializeComponent()`, `*.Designer.cs`, `System.Windows.Forms`, `Form` base class |
| **WebForms** | `*.aspx`, `*.aspx.cs`, `Page_Load`, `System.Web.UI`, `ViewState`, `IsPostBack` |
| **MVC** | `Controller` base class, `*.cshtml`, `RouteConfig`, `ActionResult`, `[HttpGet]` |
| **WCF** | `[ServiceContract]`, `[OperationContract]`, `*.svc`, `ServiceHost`, `BasicHttpBinding` |
| **Web API** | `ApiController`, `[Route]`, `IHttpActionResult`, `WebApiConfig` |
| **Console/Service** | `static void Main`, `ServiceBase`, no UI references |

## WinForms Analysis Patterns

### Event Handler Mapping
```
Button click → btnSave_Click() → Validate() → SaveToDatabase() → SP call
Form load → Form_Load() → LoadDropdowns() → BindGrids()
Grid selection → dgv_SelectionChanged() → LoadDetails() → UpdateUI()
```

### Data Binding Discovery
- Check `Designer.cs` for `DataBindings.Add()` calls
- Look for `BindingSource` components and their `DataSource` assignments
- Identify `DataGridView` column mappings and their bound properties
- Check for `DataSet`/`DataTable` typed datasets (*.xsd files)

### Form Navigation
- Search for `new FormName().Show()` or `.ShowDialog()` patterns
- Map MDI parent/child relationships
- Track `DialogResult` handling for modal flows

## WebForms Analysis Patterns

### Page Lifecycle
```
Page_Init → Page_Load → Control events → Page_PreRender → Render
```

### Key Patterns to Extract
- `Page_Load` with `!IsPostBack` — initialization vs postback logic
- `GridView`/`Repeater` data binding in `OnRowDataBound`
- `ViewState` usage for state management
- User controls (*.ascx) reuse across pages
- `Session` object usage for cross-page state
- `Response.Redirect` / `Server.Transfer` for navigation

## MVC Analysis Patterns

### Controller → Action → View Flow
- Map each controller's actions to their views
- Identify `[Authorize]` attributes for role-based access
- Check `ModelState.IsValid` patterns for validation
- Look for `TempData`/`ViewBag`/`ViewData` usage
- Identify `ActionFilter` attributes for cross-cutting concerns

## WCF Analysis Patterns

### Service Contract Discovery
- Map `[ServiceContract]` interfaces to implementations
- Document `[OperationContract]` signatures with parameters/returns
- Check `[DataContract]` / `[DataMember]` for DTO shapes
- Analyze `web.config` / `app.config` for bindings, endpoints, behaviors
- Identify `[FaultContract]` for error handling patterns

## Common .NET Framework Patterns

### Configuration Extraction
- `web.config` → `appSettings`, `connectionStrings`, `system.web`
- `app.config` → same structure for desktop apps
- Custom configuration sections
- `ConfigurationManager.AppSettings["key"]` references in code

### Data Access Identification
- **ADO.NET**: `SqlConnection`, `SqlCommand`, `SqlDataReader`, `DataAdapter`
- **Entity Framework 6**: `DbContext`, `DbSet<T>`, LINQ queries
- **Typed DataSets**: `*.xsd` files, `TableAdapter`, `Fill()` methods
- **Stored procedure calls**: `CommandType.StoredProcedure`, `@parameter` patterns

### Authentication Detection
- Windows Authentication: `WindowsIdentity`, `Thread.CurrentPrincipal`
- Forms Authentication: `FormsAuthentication.SetAuthCookie`, `[Authorize]`
- Custom auth: role checks, token validation patterns
