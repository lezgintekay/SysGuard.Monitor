namespace SysGuard.Monitor.Console;

using System.Diagnostics;
using System;
using System.Threading; 
using Models;

internal abstract class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Starting monitor... Press Ctrl+C to stop.");

        while (true)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            using var reader = process?.StandardOutput;
            var result = reader?.ReadToEnd();

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var memLine = lines[1];
                    var parts = memLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var stats = new SystemStats
                    {
                        TotalRam = parts[1],
                        UsedRam = parts[2]
                    };

                    Console.Clear();
                    DisplayDashboard(stats);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            Thread.Sleep(3000);
        }
    }


private static void DisplayDashboard(SystemStats stats)
{
    // Simple math to check usage percentage
    // Note: We need to parse strings to doubles for calculation
    double total = double.Parse(stats.TotalRam);
    double used = double.Parse(stats.UsedRam);
    double usagePercentage = (used / total) * 100;

    Console.WriteLine("======================================");
    Console.WriteLine("       SYSGUARD LIVE MONITOR          ");
    Console.WriteLine("======================================");
    Console.WriteLine($"Last Update   : {stats.CapturedAt:HH:mm:ss}");
    
    // Total RAM is usually static, so white/gray is fine
    Console.Write("Total RAM     : ");
    Console.WriteLine($"{stats.TotalRam} MB");

    // Used RAM with Color Indicator
    Console.Write("Used RAM      : ");
    
    if (usagePercentage > 80) 
    {
        Console.ForegroundColor = ConsoleColor.Red; // Critical
    }
    else if (usagePercentage > 50)
    {
        Console.ForegroundColor = ConsoleColor.Yellow; // Warning
    }
    else 
    {
        Console.ForegroundColor = ConsoleColor.Green; // Healthy
    }

    Console.WriteLine($"{stats.UsedRam} MB ({usagePercentage:F1}%)");
    Console.ResetColor(); // Important: Reset color so the rest of the UI stays normal

    Console.WriteLine("======================================");
    
    if (usagePercentage > 80)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ALERT: HIGH MEMORY USAGE DETECTED!  ");
        Console.ResetColor();
        Console.WriteLine("======================================");
    }
    
    Console.WriteLine("Press Ctrl+C to stop");
}
}