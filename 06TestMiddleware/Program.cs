var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseMiddleware<TestMiddleware>("666",123,"hello");

app.Run(async context =>
{
    await context.Response.WriteAsync("success!");
});

app.Run();


public class TestMiddleware
{
    readonly RequestDelegate _next;
    readonly string _arg1;
    readonly int _arg2;
    readonly string _arg3;

    public TestMiddleware(RequestDelegate next,string arg1,int arg2,string arg3)
    {
        _next = next;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
    }
    public async Task InvokeAsync(HttpContext ctx)
    {
        Console.WriteLine($"自定义中间件调用{_arg1},{_arg2.ToString()},{_arg3}");
        await _next(ctx);
    }
}