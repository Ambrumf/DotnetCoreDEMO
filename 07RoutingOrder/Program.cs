var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (context, next) =>
{
    string? endpoint = context.GetEndpoint()?.DisplayName;
    if (endpoint is not null)
    {
        Console.WriteLine(endpoint);
    }
    else
    {
        System.Console.WriteLine("null");
    }
    await next();
});

app.MapGet("/user", () => System.Console.WriteLine("endpoint here"));
 
app.Use(async (context, next) =>
{
    string str = "hello";
    Console.WriteLine(str);
    await next();
});

app.Run();
        