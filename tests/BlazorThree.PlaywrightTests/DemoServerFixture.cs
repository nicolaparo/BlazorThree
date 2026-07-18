using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorThree.PlaywrightTests;

public sealed class DemoServerFixture : IAsyncLifetime
{
    private WebApplication? app;

    public string BaseUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var projectRoot = GetProjectRoot();
        BaseUrl = $"http://127.0.0.1:{GetFreeTcpPort()}";

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(DemoServerFixture).Assembly.GetName().Name,
            ContentRootPath = projectRoot,
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseStaticWebAssets();
        builder.WebHost.UseUrls(BaseUrl);
        builder.Logging.SetMinimumLevel(LogLevel.Information);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();

        try
        {
            app = builder.Build();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapStaticAssets();
            app.MapBlazorHub();
            app.MapRazorPages();
            await app.StartAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to start Playwright demo host at {BaseUrl}.", ex);
        }

        await WaitForServerAsync();
    }

    public async Task DisposeAsync()
    {
        if (app is null)
        {
            return;
        }

        try
        {
            await app.StopAsync();
        }
        finally
        {
            await app.DisposeAsync();
            app = null;
        }
    }

    private async Task WaitForServerAsync()
    {
        using var httpClient = new HttpClient();
        var timeoutAt = DateTime.UtcNow.AddSeconds(45);
        Exception? lastError = null;

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                using var response = await httpClient.GetAsync(BaseUrl);
                if ((int)response.StatusCode > 0)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException($"Timed out waiting for demo server at {BaseUrl}. Last error: {lastError?.Message}");
    }

    private static string GetProjectRoot()
    {
        var current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current))
        {
            if (File.Exists(Path.Combine(current, "BlazorThree.PlaywrightTests.csproj")))
            {
                return current;
            }

            current = Directory.GetParent(current)?.FullName ?? string.Empty;
        }

        throw new DirectoryNotFoundException("Could not locate Playwright test project root.");
    }

    private static int GetFreeTcpPort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }
}
