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
builder.Services.AddSingleton<IPathfindingAlgorithm, DFS>();
builder.Services.AddSingleton<IPathfindingAlgorithm, Dijkstra>();
builder.Services.AddSingleton<IPathfindingAlgorithm, BidirectionalSearch>();
builder.Services.AddSingleton<IPathfindingAlgorithm, AStar>();

builder.Services.AddSingleton<PathfindingAlgorithmFactory>();
builder.Services.AddSingleton<PathfindingExecutionManager>();

builder.Services.AddSignalR(options => options.MaximumParallelInvocationsPerClient = 2).AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();

app.UseCors("Frontend");
app.MapGet("/", () => Results.Ok(new { name = "AlgoVision API", status = "ready" }));
app.MapHub<PathfindingHub>("/pathfindingHub");
app.Run();