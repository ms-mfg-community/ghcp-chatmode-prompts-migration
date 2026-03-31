namespace AppModernization.Web;

public static class HealthCheckEndpoints
{
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = typeof(HealthCheckEndpoints).Assembly.GetName().Version?.ToString() ?? "1.0.0"
        }))
        .ExcludeFromDescription();

        return app;
    }
}
