var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async(context, next) =>
{
    context.Response.Headers.Add("X-tag", "Demo");
    await next();
});

app.Use(async (context, next) =>
{
    Console.WriteLine("Use RequestDelegate");
    await next(context);
});


app.Use(async (context, next) =>
{
    var request = context.Request;
    var response = context.Response;
    if (request.Query["flag"] == "1")
    {
        await response.WriteAsync("Flag = 1");
    }
    else await next();
});

app.Run();
