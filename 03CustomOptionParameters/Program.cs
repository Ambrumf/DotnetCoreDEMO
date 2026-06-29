using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IOptionsFactory<DatabaseOptions>, DatabaseOptionFactory>();

var app = builder.Build();

app.MapGet("/", () => {
    StringBuilder sb = new();
    DatabaseOptions dbo = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

    sb.AppendLine(dbo.url);
    sb.AppendLine(dbo.MaxConnetions.ToString());

    return sb.ToString();
});

app.Run();

public class DatabaseOptionFactory: OptionsFactory<DatabaseOptions>
{
    public DatabaseOptionFactory(
        IEnumerable<IConfigureOptions<DatabaseOptions>> cfgs,
        IEnumerable<IPostConfigureOptions<DatabaseOptions>> pcfgs,
        IEnumerable<IValidateOptions<DatabaseOptions>> vos
    ) : base(cfgs, pcfgs, vos) { }

    protected override DatabaseOptions CreateInstance(string name)
    {
        //return (DatabaseOptions)Activator.CreateInstance(typeof(DatabaseOptions),"localhost",50);
        return new DatabaseOptions("localhost", 50);
    }
}
public class DatabaseOptions
{
    public DatabaseOptions(string u,int connectionNums)
    {
        url = u;
        MaxConnetions = connectionNums;
    }
    public string? url { get; set; }
    public int MaxConnetions { get; set; }
}