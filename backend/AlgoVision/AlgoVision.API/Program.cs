using AlgoVision.API.Algorithms;
using AlgoVision.API.Hubs;
using AlgoVision.API.Interfaces;
using AlgoVision.API.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var frontendOrigin = builder.Configuration["FrontendOrigin"];

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddSingleton<IPathfindingAlgorithm, BFS>();
builder.Services.AddSingleton<PathfindingAlgorithmFactory>();

builder.Services.AddSignalR().AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseCors("Frontend");
app.MapGet("/", () => Results.Ok(new { name = "AlgoVision API", status = "ready" }));
app.MapHub<PathfindingHub>("/pathfindingHub");
app.Run();
