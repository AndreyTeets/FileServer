using System.Text.Json;
using FileServer.Configuration;
using FileServer.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Settings>(builder.Configuration.GetSection(nameof(Settings)));
builder.Services.AddSingleton<FileService>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

WebApplication app = builder.Build();

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

app.MapControllers();

app.Use(async (context, next) =>
{
    if (context.Request.Method == HttpMethod.Get.Method)
        context.Response.Headers["Cache-Control"] = "no-cache, no-store, private";
    await next();
});

app.Run();
