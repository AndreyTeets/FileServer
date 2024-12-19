using FileServer;
using FileServer.Auth;
using FileServer.Configuration;
using FileServer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.ConfigureSettings();
builder.ConfigureLogging();
builder.ConfigureKestrel();

builder.Services.AddSingleton<FileService>();
builder.Services.AddSingleton<TokenService>();

builder.Services.AddAuthentication()
    .AddScheme<DoubleTokenAuthenticationSchemeOptions, DoubleTokenAuthenticationHandler>(
        Constants.DoubleTokenAuthenticationSchemeName, options => { });

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = StaticSettings.JsonOptions.PropertyNamingPolicy;
});

WebApplication app = builder.Build();
app.SetupSettingsMonitor();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Value == "/")
        context.Request.Path = "/index.html";
    await next();
});
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = sfrContext =>
    {
        sfrContext.Context.Response.Headers["Cache-Control"] = "no-cache, no-store";
    },
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethod.Get.Method)
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, private";
    await next();
});

app.Run();
