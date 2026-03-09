using Microsoft.EntityFrameworkCore;
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

app.MapControllers();

Console.WriteLine("--------------------------------------");
Console.WriteLine("   SYSGUARD API IS RUNNING (PORT 5005)");
Console.WriteLine("--------------------------------------");

app.Run("http://0.0.0.0:5005");