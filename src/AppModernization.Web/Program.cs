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

// Authentication: Cookie scheme as default, GitHub OAuth as external provider
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "GitHub";
})
.AddCookie(options =>
{
    options.LoginPath = "/"; // Don't redirect — we handle auth in the UI
    options.Cookie.Name = "AppMod.Auth";
})
.AddGitHub("GitHub", options =>
{
    // Works for both GitHub OAuth Apps and GitHub Apps (with user authorization)
    options.ClientId = builder.Configuration["GitHub:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"] ?? "";
    options.Scope.Add("read:user");
    options.Scope.Add("user:email");
    options.SaveTokens = true;
    options.Events.OnCreatingTicket = context =>
    {
        var avatarUrl = context.User.GetProperty("avatar_url").GetString();
        if (!string.IsNullOrEmpty(avatarUrl))
        {
            context.Identity?.AddClaim(new Claim("urn:github:avatar", avatarUrl));
        }
        context.Identity?.AddClaim(new Claim("urn:github:auth_method", "oauth"));
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

// --- Auth endpoints ---

// GitHub OAuth / GitHub App login
app.MapGet("/auth/login", (string? returnUrl) =>
    Results.Challenge(new AuthenticationProperties
    {
        RedirectUri = returnUrl ?? "/"
    }, ["GitHub"]));

// PAT-based login — creates a cookie session from a personal access token
app.MapPost("/auth/pat-login", async (HttpContext ctx) =>
{
    var form = await ctx.Request.ReadFormAsync();
    var pat = form["pat"].ToString();

    if (string.IsNullOrWhiteSpace(pat))
        return Results.Redirect("/?error=pat_empty");

    // Validate PAT against GitHub API
    using var http = new HttpClient();
    http.DefaultRequestHeaders.Add("Authorization", $"Bearer {pat}");
    http.DefaultRequestHeaders.Add("User-Agent", "AppModernization");
    var response = await http.GetAsync("https://api.github.com/user");

    if (!response.IsSuccessStatusCode)
        return Results.Redirect("/?error=pat_invalid");

    var user = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
    var login = user.GetProperty("login").GetString() ?? "unknown";
    var avatar = user.GetProperty("avatar_url").GetString() ?? "";
    var name = user.TryGetProperty("name", out var n) ? n.GetString() ?? login : login;

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, name),
        new(ClaimTypes.NameIdentifier, login),
        new("urn:github:avatar", avatar),
        new("urn:github:auth_method", "pat"),
        new("urn:github:token", pat)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    return Results.Redirect("/");
}).DisableAntiforgery();

// Logout
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
