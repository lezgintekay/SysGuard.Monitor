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
        Console.WriteLine("======================================");
        Console.WriteLine("       SYSGUARD LIVE MONITOR          ");
        Console.WriteLine("======================================");
        Console.WriteLine($"Last Update   : {stats.CapturedAt:HH:mm:ss}");
        Console.WriteLine($"Total RAM     : {stats.TotalRam} MB");
        Console.WriteLine($"Used RAM      : {stats.UsedRam} MB");
        Console.WriteLine("======================================");
        Console.WriteLine("Press Ctrl+C to stop");
    }
}