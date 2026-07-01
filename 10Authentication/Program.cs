using _10Authentication.Authentication;
using _10Authentication.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDemoAuthentication(builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapDemoEndpoints();

app.Run();
