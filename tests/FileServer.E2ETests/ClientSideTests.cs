using FileServer.E2ETests.Snapshots;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace FileServer.E2ETests;

[TestFixture]
internal sealed class ClientSideTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions() => new()
    {
        IgnoreHTTPSErrors = true,
    };

    [SetUp]
    public void SetUpTimeouts()
    {
        SetDefaultExpectTimeout(1_000); // Timeouts for Expect.ToXXX
        Page.SetDefaultTimeout(3_000); // Timeouts for Page.[ClickAsync|FillAsync|SetFilesAsync|e.t.c]
        Page.SetDefaultNavigationTimeout(10_000); // Timeouts for Page.[GotoAsync|ReloadAsync|e.t.c]
    }

    [TearDown]
    public void WriteServerOutput()
    {
        TestContext.Out.WriteLine(StartServerFixture.GetServerOutputFunc());
    }

    [Test]
    public async Task AllPages_FunctionCorrectly()
    {
        await OpenInitialPage();
        await Expect(Page).ToHaveTitleAsync("FileServer");
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterOpenInitialPageSnapshot());

        Assert.That(await GetContentForLastFileOnDownloadPage(), Is.EqualTo("test_anonfile1_content"));
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterOpenDownloadPageNoAuthSnapshot());

        await Login();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterLoginSnapshot());

        await UploadTestFile();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterUploadFileSuccessfullySnapshot());

        await UploadTestFile();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterUploadFileWithErrorSnapshot());

        Assert.That(await GetContentForLastFileOnDownloadPage(), Is.EqualTo("test_file1_content"));
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterOpenDownloadPageWithAuthSnapshot());

        await Logout();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(new AfterLogoutSnapshot());
    }

    private async Task OpenInitialPage() =>
        await Page.GotoAsync("https://localhost:9443");

    private async Task Login()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "AuthPage" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).FillAsync("123456789012");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }

    private async Task UploadTestFile()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "UploadPage" }).ClickAsync();
        IFileChooser fileChooser = await Page.RunAndWaitForFileChooserAsync(async () =>
            await Page.Locator("#root input[type='file']").ClickAsync());
        await fileChooser.SetFilesAsync(["test_upload_file.txt"]);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Upload", Exact = true }).ClickAsync();
    }

    private async Task<string> GetContentForLastFileOnDownloadPage()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "DownloadPage" }).ClickAsync();
        IDownload download = await Page.RunAndWaitForDownloadAsync(async () =>
            await Page.Locator("#root table tbody tr").Last.Locator("td").Nth(3).ClickAsync());
        using StreamReader sr = new(await download.CreateReadStreamAsync());
        return await sr.ReadToEndAsync();
    }

    private async Task Logout()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "AuthPage" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
    }
}
