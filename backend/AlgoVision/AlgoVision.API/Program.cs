using AlgoVision.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var frontendOrigin = builder.Configuration["FrontendOrigin"];

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));


builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("Frontend");
app.MapGet("/", () => Results.Ok(new { name = "AlgoVision API", status = "ready" }));
app.MapHub<PathfindingHub>("/pathfindingHub");
app.Run();
