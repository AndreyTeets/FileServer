using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using S = FileServer.E2ETests.Snapshots.SnapshotValues;

namespace FileServer.E2ETests;

[TestFixture]
public class ClientSideTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new()
        {
            IgnoreHTTPSErrors = true,
        };
    }

    [SetUp]
    public void SetupTimeouts()
    {
        SetDefaultExpectTimeout(1_000);
        Page.SetDefaultNavigationTimeout(1_000);
        Page.SetDefaultTimeout(1_000);
    }

    [TearDown]
    public void WriteServerOutput()
    {
        if (StartServerFixture.GetServerOutputFunc is not null)
            TestContext.Out.WriteLine(StartServerFixture.GetServerOutputFunc());
    }

    [Test]
    public async Task AllPages_FunctionCorrectly()
    {
        await OpenInitialPage();
        await Expect(Page).ToHaveTitleAsync("Files");
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_OpenInitialPage);

        Assert.That(await GetContentForLastFileOnDownloadPage(), Is.EqualTo("test_anonfile1_content"));
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_OpenDownloadPageNoAuth);

        await Login();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_Login);

        await UploadTestFile();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_UploadFileSuccessfully);

        await UploadTestFile();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_UploadFileWithError);

        Assert.That(await GetContentForLastFileOnDownloadPage(), Is.EqualTo("test_file1_content"));
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_OpenDownloadPageWithAuth);

        await Logout();
        await Expect(Page.Locator("body")).ToMatchAriaSnapshotAsync(S.After_Logout);
    }

    private async Task OpenInitialPage()
    {
        await Page.GotoAsync("https://localhost:9443");
    }

    private async Task Login()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "AuthPage" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).FillAsync("012345678912");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Login" }).ClickAsync();
    }

    private async Task UploadTestFile()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "UploadPage" }).ClickAsync();
        IFileChooser fileChooser = await Page.RunAndWaitForFileChooserAsync(async () =>
        {
            await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        });
        await fileChooser.SetFilesAsync(["test_upload_file.txt"]);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Upload", Exact = true }).ClickAsync();
    }

    private async Task<string> GetContentForLastFileOnDownloadPage()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "DownloadPage" }).ClickAsync();
        IDownload download = await Page.RunAndWaitForDownloadAsync(async () =>
        {
            await Page.Locator("#app table tbody tr").Last.Locator("td").Nth(3).ClickAsync();
        });
        using StreamReader sr = new(await download.CreateReadStreamAsync());
        return await sr.ReadToEndAsync();
    }

    private async Task Logout()
    {
        await Page.GetByRole(AriaRole.Button, new() { Name = "AuthPage" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
    }
}
