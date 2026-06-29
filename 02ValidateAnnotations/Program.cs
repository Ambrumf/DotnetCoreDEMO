using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<DatabaseOptions>()
    .Configure(o =>
    {
        o.url = "https://localhost:3306";
    })
    .Bind(builder.Configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();



var app = builder.Build();

app.MapGet("/", () =>
{
    try
    {
        StringBuilder sb = new();
        DatabaseOptions dbo = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        sb.AppendLine(dbo.url);
        sb.AppendLine(dbo.MaxConnetions.ToString());

        return sb.ToString();
    }
    catch (Exception e)
    {
        return $"Error: {e.Message} \nWhen data requested";
    }
});


try
{
    app.Run();
}
catch (Exception e)
{
    System.Console.WriteLine($"Error: {e.Message} \nWhen app started");
}

public class DatabaseOptions
{
    [Required]
    public string? url{ get; set; }
    [Range(1,20)]
    public int MaxConnetions{ get; set; }
}