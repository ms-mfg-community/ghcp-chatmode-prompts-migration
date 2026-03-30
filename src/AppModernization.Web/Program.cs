using AppModernization.Web;
using AppModernization.Web.Components;
using AppModernization.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// GitHub OAuth authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie()
.AddGitHub("GitHub", options =>
{
    options.ClientId = builder.Configuration["GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? "";
    options.Scope.Add("read:user");
    options.Scope.Add("user:email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        // Map GitHub avatar URL to a claim for display
        var avatarUrl = context.User.GetProperty("avatar_url").GetString();
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            context.Identity?.AddClaim(new Claim("urn:github:avatar", avatarUrl));
        }
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// App Modernization services
builder.Services.AddSingleton<AgentPromptService>();
builder.Services.AddSingleton<CopilotService>();
builder.Services.AddSingleton<ProjectPersistenceService>();
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

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Auth endpoints
app.MapGet("/auth/login", (string? returnUrl) =>
    Results.Challenge(new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/"
    }, ["GitHub"]));

app.MapGet("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapStaticAssets();
app.MapHealthCheckEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
