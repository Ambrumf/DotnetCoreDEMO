var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<TestMiddleware>();

app.Run(async context =>
{
    await context.Response.WriteAsync("success!");
});

app.Run();


public class TestMiddleware
{
    readonly RequestDelegate _next;

    public TestMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext ctx)
    {
        Console.WriteLine("自定义中间件调用");
        await _next(ctx);
    }
}