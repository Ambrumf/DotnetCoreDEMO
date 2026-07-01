var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/",async context =>
{
    var request = context.Request;
    var response = context.Response;
    response.Headers.ContentType = "text/plain;charset=UTF-8";
    if (request.HasFormContentType)
    {
        var postData = from p in request.Form
                       select $"{p.Key} = {p.Value}";
        await response.WriteAsync(string.Join('\n', postData));
        return;
    }

    await response.WriteAsJsonAsync("未发现消息正文");
});

app.Run();


