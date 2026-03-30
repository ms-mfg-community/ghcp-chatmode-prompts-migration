using AppModernization.Web.Components;
using AppModernization.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// App Modernization services
builder.Services.AddSingleton<AgentPromptService>();
builder.Services.AddSingleton<CopilotService>();
builder.Services.AddScoped<MigrationStateService>();

var app = builder.Build();

// Initialize Copilot SDK (non-blocking — will start when first session is created)
_ = Task.Run(async () =>
{
    try
    {
        var copilot = app.Services.GetRequiredService<CopilotService>();
        await copilot.InitializeAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Copilot SDK initialization deferred — CLI may not be available");
    }
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
