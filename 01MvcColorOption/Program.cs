
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ThemeOptions>("theme-1", topt =>
{
    topt.BackColor = "red";
    topt.ContentColor = "black";
});

builder.Services.Configure<ThemeOptions>("theme-2", topt =>
{
    topt.BackColor = "black";
    topt.ContentColor = "lightgreen";
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
public class ThemeOptions
{
    public string? BackColor { get; set; }
    public string? ContentColor { get; set; }
}


