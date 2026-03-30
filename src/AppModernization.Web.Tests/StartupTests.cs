using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace AppModernization.Web.Tests;

public class StartupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StartupTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task App_Starts_And_Home_Returns_Success()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/");

        Assert.True(response.IsSuccessStatusCode,
            $"Home page returned {response.StatusCode}");
    }

    [Fact]
    public async Task Health_Endpoint_Returns_Ok()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("healthy", body);
    }

    [Fact]
    public async Task Dashboard_Returns_Success()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/dashboard");

        Assert.True(response.IsSuccessStatusCode,
            $"Dashboard returned {response.StatusCode}");
    }

    [Theory]
    [InlineData("/phase0/discover")]
    [InlineData("/phase0/specify")]
    [InlineData("/phase0/validate")]
    [InlineData("/phase1/plan")]
    [InlineData("/phase2/assess")]
    [InlineData("/phase3/migrate")]
    [InlineData("/phase4/infra")]
    [InlineData("/phase5/deploy")]
    [InlineData("/phase6/cicd")]
    [InlineData("/reports")]
    [InlineData("/projects")]
    public async Task Phase_Pages_Return_Success(string url)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync(url);

        Assert.True(response.IsSuccessStatusCode,
            $"{url} returned {response.StatusCode}");
    }

    [Fact]
    public async Task Auth_Login_Without_OAuth_Configured_Returns_Error()
    {
        // When OAuth is not configured, /auth/login should not crash the app
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/auth/login");

        // Should redirect or return an error, but NOT throw 500
        Assert.NotEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task Auth_Logout_Redirects_To_Home()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/auth/logout");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }
}
