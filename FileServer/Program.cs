using FileServer.Configuration;

if (args.Length > 0)
{
    Utility.ShowVersionAndUsage();
    return;
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.ConfigureSettings();
builder.ConfigureLogging();
builder.ConfigureKestrel();

builder.Services.AddAndConfigureServices();

WebApplication app = builder.Build();
app.SetupSettingsMonitor();

app.UseToIndexPageRedirect();
app.UseStaticFilesWithNoCacheHeaders();
app.UseAuthentication();
app.UseAuthorization();
app.UseControllersWithAuthorization();
app.UseNoCacheHeaders();

app.Run();
