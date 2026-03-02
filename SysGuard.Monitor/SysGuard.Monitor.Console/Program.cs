namespace SysGuard.Monitor.Console; 

using System.Diagnostics;
using System;
using System.IO; 
using System.Threading; 
using Models;
using System.Linq;

internal abstract class Program
{
    private static (long Idle, long Total) GetCpuTimes()
    {
        var line = File.ReadLines("/proc/stat").First();
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var idle = long.Parse(parts[4]);
        var total = parts.Skip(1).Take(7).Select(long.Parse).Sum();
        
        return (idle, total);
    }

    private static void Main()
    {
        Console.WriteLine("Starting high-precision monitor...");

        while (true)
        {
            // 1. CPU Measurement
            var t1 = GetCpuTimes();
            Thread.Sleep(500); 
            var t2 = GetCpuTimes();

            double idleDiff = t2.Idle - t1.Idle;
            double totalDiff = t2.Total - t1.Total;
            double cpuUsage = (1.0 - (idleDiff / totalDiff)) * 100.0;

            // 2. RAM Data
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

            // 3. Uptime Data 
            var uptimePsi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"uptime -p\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var uptimeProcess = Process.Start(uptimePsi);
            var uptimeResult = uptimeProcess?.StandardOutput.ReadToEnd().Trim().Replace("up ", "");

            // 4. Disk Usage Data 
            var diskPsi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"df -h / | awk 'NR==2 {print $5}'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var diskProcess = Process.Start(diskPsi);
            var diskResult = diskProcess?.StandardOutput.ReadToEnd().Trim().Replace("%", "");

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
                        CpuUsage = cpuUsage.ToString("F1"),
                        Uptime = uptimeResult ?? "Unknown", // Set Uptime
                        DiskUsage = diskResult ?? "0"       // Set Disk Usage
                    };

                    Console.Clear();
                    DisplayDashboard(stats);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing data: {ex.Message}");
                }
            }
            
            Thread.Sleep(500); 
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private static void DisplayDashboard(SystemStats stats)
    {
        var total = double.Parse(stats.TotalRam);
        var used = double.Parse(stats.UsedRam);
        var ramPercentage = (used / total) * 100;
        
        if (!double.TryParse(stats.CpuUsage, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double cpuPercentage))
        {
            cpuPercentage = 0;
        }

        var diskPercentage = double.TryParse(stats.DiskUsage, out var d) ? d : 0;

        Console.WriteLine("======================================");
        Console.WriteLine("       SYSGUARD LIVE MONITOR          ");
        Console.WriteLine("======================================");
        Console.WriteLine($"Last Update   : {stats.CapturedAt:HH:mm:ss}");
        Console.WriteLine($"System Uptime : {stats.Uptime}"); // Display Uptime
        Console.WriteLine("--------------------------------------");
        
        // CPU
        Console.Write("CPU Usage     : ");
        ApplyColorCoding(cpuPercentage);
        Console.WriteLine($"{cpuPercentage:F1}%");
        Console.ResetColor();
        
        // RAM
        Console.Write("Used RAM      : ");
        ApplyColorCoding(ramPercentage);
        Console.WriteLine($"{stats.UsedRam} MB ({ramPercentage:F1}%)");
        Console.ResetColor();

        // DISK (NEW)
        Console.Write("Disk Usage    : ");
        ApplyColorCoding(diskPercentage);
        Console.WriteLine($"{diskPercentage}%");
        Console.ResetColor();

        Console.WriteLine("======================================");
        
        if (ramPercentage > 80 || cpuPercentage > 80 || diskPercentage > 80)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ALERT: HIGH RESOURCE USAGE!         ");
            Console.ResetColor();
            Console.WriteLine("======================================");
        }
        
        Console.WriteLine("Press Ctrl+C to stop");
    }

    private static void ApplyColorCoding(double percentage)
    {
        Console.ForegroundColor = percentage switch
        {
            > 80 => ConsoleColor.Red,
            > 50 => ConsoleColor.Yellow,
            _ => ConsoleColor.Green
        };
    }
}