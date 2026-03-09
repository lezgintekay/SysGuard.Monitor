using Microsoft.EntityFrameworkCore;
using SysGuard.Monitor.Models;
using SysGuard.Monitor.API.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseDefaultFiles(); 
app.UseStaticFiles();  

app.UseCors("AllowAll");

SystemStats? latestStats = null;

app.MapPost("/stats", (SystemStats stats) => 
{
    latestStats = stats;
    latestStats.CapturedAt = DateTime.Now;
    return Results.Ok(new { message = "Data received successfully" });
});

app.MapGet("/stats", () => 
{
    if (latestStats == null) return Results.NotFound(new { message = "No data received yet." });
    return Results.Ok(latestStats);
});


app.Run("http://0.0.0.0:5005");