using Microsoft.Playwright;

namespace BlazorThree.Tests;

public sealed class SceneRuntimeTests(DemoServerFixture fixture) : IClassFixture<DemoServerFixture>
{
    [Fact]
    public async Task PlaywrightScenePage_RendersCanvas_AndHostHoverCallbacksFire()
    {
        await using var session = await CreateSessionAsync();
        var page = session.Page;

        await page.GotoAsync($"{fixture.BaseUrl}/playwright-scene-tests");

        var canvas = await WaitForCanvasAsync(session);

        await canvas.HoverAsync();
        await Assertions.Expect(page.Locator("#scene-enter-count")).ToHaveTextAsync("1");

        await page.Locator("#toggle-cube").HoverAsync();
        await Assertions.Expect(page.Locator("#scene-leave-count")).ToHaveTextAsync("1");
    }

    [Fact]
    public async Task PlaywrightScenePage_ClickingCenteredMeshDispatchesSceneAndMeshCallbacks()
    {
        await using var session = await CreateSessionAsync();
        var page = session.Page;

        await page.GotoAsync($"{fixture.BaseUrl}/playwright-scene-tests");

        var canvas = await WaitForCanvasAsync(session);

        await canvas.ClickAsync(new LocatorClickOptions
        {
            Position = new Position { X = 240, Y = 180 }
        });

        await Assertions.Expect(page.Locator("#scene-click-count")).ToHaveTextAsync("1");
        await Assertions.Expect(page.Locator("#cube-click-count")).ToHaveTextAsync("1");
        await Assertions.Expect(page.Locator("#last-picked-element")).ToHaveTextAsync("cube");
    }

    [Fact]
    public async Task PlaywrightScenePage_HidingCubeStopsFurtherMeshClicks()
    {
        await using var session = await CreateSessionAsync();
        var page = session.Page;

        await page.GotoAsync($"{fixture.BaseUrl}/playwright-scene-tests");

        var canvas = await WaitForCanvasAsync(session);

        var clickPosition = new Position { X = 240, Y = 180 };
        await canvas.ClickAsync(new LocatorClickOptions { Position = clickPosition });
        await Assertions.Expect(page.Locator("#cube-click-count")).ToHaveTextAsync("1");

        await page.GetByRole(AriaRole.Button, new() { Name = "Toggle Cube" }).ClickAsync();
        await Assertions.Expect(page.Locator("#cube-visible")).ToHaveTextAsync("false");

        await canvas.ClickAsync(new LocatorClickOptions { Position = clickPosition });
        await Assertions.Expect(page.Locator("#cube-click-count")).ToHaveTextAsync("1");
        await Assertions.Expect(page.Locator("#scene-click-count")).ToHaveTextAsync("2");
    }

    private static async Task<BrowserSession> CreateSessionAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();
        return new BrowserSession(playwright, browser, context, page);
    }

    private static async Task<ILocator> WaitForCanvasAsync(BrowserSession session)
    {
        var canvas = session.Page.Locator("#playwright-scene-shell canvas");

        try
        {
            await Assertions.Expect(canvas).ToBeVisibleAsync(new() { Timeout = 15000 });
            return canvas;
        }
        catch (PlaywrightException ex)
        {
            var shellHtml = await session.Page.Locator("#playwright-scene-shell").InnerHTMLAsync();
            var pageHtml = await session.Page.ContentAsync();
            throw new Xunit.Sdk.XunitException($"Canvas did not appear. Shell HTML: {shellHtml}\nPage HTML: {pageHtml}\nConsole: {string.Join(" | ", session.ConsoleMessages)}\nPageErrors: {string.Join(" | ", session.PageErrors)}\nOriginal: {ex.Message}");
        }
    }

    private sealed class BrowserSession : IAsyncDisposable
    {
        public BrowserSession(IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page)
        {
            Playwright = playwright;
            Browser = browser;
            Context = context;
            Page = page;

            Page.Console += (_, message) => ConsoleMessages.Add($"{message.Type}: {message.Text}");
            Page.PageError += (_, message) => PageErrors.Add(message);
        }

        public IPlaywright Playwright { get; }

        public IBrowser Browser { get; }

        public IBrowserContext Context { get; }

        public IPage Page { get; }

        public List<string> ConsoleMessages { get; } = [];

        public List<string> PageErrors { get; } = [];

        public async ValueTask DisposeAsync()
        {
            await Context.CloseAsync();
            await Browser.DisposeAsync();
            Playwright.Dispose();
        }
    }
}
