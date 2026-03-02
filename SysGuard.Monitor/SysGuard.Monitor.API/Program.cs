using SysGuard.Monitor.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


app.MapGet("/stats", () => new SystemStats 
{ 
    CpuUsage = "0.0", 
    UsedRam = "0", 
    Uptime = "API is running..." 
});

app.Run("http://0.0.0.0:5005");