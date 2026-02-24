namespace SysGuard.Monitor.Console;

using System.Diagnostics;
using System;
using System.IO; // For file operations
using System.Threading; 
using Models;
using System.Linq;

internal abstract class Program
{
    private static (long Idle, long Total) GetCpuTimes()
    {
        // Read the file containing Linux system statistics
        var line = File.ReadLines("/proc/stat").First();
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Sum the first 7 values: user, nice, system, idle, iowait, irq, softirq
        long idle = long.Parse(parts[4]);
        long total = parts.Skip(1).Take(7).Select(long.Parse).Sum();
        
        return (idle, total);
    }

    private static void Main(string[] args)
    {
        Console.WriteLine("Starting high-precision monitor...");

        while (true)
        {
            // First measurement
            var t1 = GetCpuTimes();
            Thread.Sleep(500); // Wait half a second
            
            // Second measurement
            var t2 = GetCpuTimes();

            // Calculate the difference
            double idleDiff = t2.Idle - t1.Idle;
            double totalDiff = t2.Total - t1.Total;
            double cpuUsage = (1.0 - (idleDiff / totalDiff)) * 100.0;

            // RAM Data (Keeping the existing working command)
            var ramPsi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"free -m\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var ramProcess = Process.Start(ramPsi);
            var ramResult = ramProcess?.StandardOutput.ReadToEnd();

            if (!string.IsNullOrEmpty(ramResult))
            {
                try
                {
                    var lines = ramResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var parts = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var stats = new SystemStats
                    {
                        TotalRam = parts[1],
                        UsedRam = parts[2],
                        CpuUsage = cpuUsage.ToString("F1") // Assigning the calculated precision value
                    };

                    Console.Clear();
                    DisplayDashboard(stats);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing data: {ex.Message}");
                }
            }
            
            Thread.Sleep(500); // Total 1 second update interval
        }
    }

    private static void DisplayDashboard(SystemStats stats)
    {
        // Numeric conversions
        double total = double.Parse(stats.TotalRam);
        double used = double.Parse(stats.UsedRam);
        double ramPercentage = (used / total) * 100;
        
        // Safely parse CPU data to double using InvariantCulture
        if (!double.TryParse(stats.CpuUsage, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cpuPercentage))
        {
            cpuPercentage = 0;
        }

        Console.WriteLine("======================================");
        Console.WriteLine("       SYSGUARD LIVE MONITOR          ");
        Console.WriteLine("======================================");
        Console.WriteLine($"Last Update   : {stats.CapturedAt:HH:mm:ss}");
        
        // CPU Display
        Console.Write("CPU Usage     : ");
        ApplyColorCoding(cpuPercentage);
        Console.WriteLine($"{cpuPercentage:F1}%");
        Console.ResetColor();
        
        // RAM Display
        Console.Write("Used RAM      : ");
        ApplyColorCoding(ramPercentage);
        Console.WriteLine($"{stats.UsedRam} MB ({ramPercentage:F1}%)");
        Console.ResetColor();

        Console.WriteLine("======================================");
        
        if (ramPercentage > 80 || cpuPercentage > 80)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ALERT: HIGH RESOURCE USAGE!         ");
            Console.ResetColor();
            Console.WriteLine("======================================");
        }
        
        Console.WriteLine("Press Ctrl+C to stop");
    }

    // Color coding logic based on percentage
    private static void ApplyColorCoding(double percentage)
    {
        if (percentage > 80) Console.ForegroundColor = ConsoleColor.Red;
        else if (percentage > 50) Console.ForegroundColor = ConsoleColor.Yellow;
        else Console.ForegroundColor = ConsoleColor.Green;
    }
}