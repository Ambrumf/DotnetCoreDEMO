using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (context, next) =>
{
    var request = context.Request;
    SHA256 sha = SHA256.Create();
    byte[] data = await sha.ComputeHashAsync(request.Body);

    context.Items.Add("result", data);
    
    await next();
});

app.Run(async (context) =>
{
    var response = context.Response;
    response.Headers.ContentType = "text/html:charset=UTF-8";
    byte[]? data = (byte[]?)context.Items["result"];
    string msg = String.Empty;
    if (data is not null)
    {
        msg = $"sha256: ({Convert.ToBase64String(data)})";
    }
    else msg = "未检测到数据";
    byte[] output = Encoding.UTF8.GetBytes(msg);
    response.ContentLength = output.Length;

    response.Cookies.Append("key1", "value1");

    CookieOptions opt = new()
    {
        Expires = DateTime.Now.AddSeconds(15)
    };
    response.Cookies.Append("key2", "value2",opt);

    await response.Body.WriteAsync(output);
});

app.Run();
