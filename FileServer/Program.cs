WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
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

app.Run();
