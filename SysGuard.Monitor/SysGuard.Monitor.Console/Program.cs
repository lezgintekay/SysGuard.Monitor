namespace SysGuard.Monitor.Console;

using System.Diagnostics;
using System;
using System.IO; 
using System.Threading; 
using SysGuard.Monitor.Models; 
using System.Linq;
using System.Net.Http.Json; 

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

    private static async Task Main()
    {
        System.Console.WriteLine("Starting SysGuard Agent...");
        using var client = new HttpClient(); 

        while (true)
        {
            var t1 = GetCpuTimes();
            Thread.Sleep(500); 
            var t2 = GetCpuTimes();

            double idleDiff = t2.Idle - t1.Idle;
            double totalDiff = t2.Total - t1.Total;
            double cpuUsage = (1.0 - (idleDiff / totalDiff)) * 100.0;

            var ramPsi = new ProcessStartInfo { FileName = "/bin/bash", Arguments = "-c \"free -m\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var ramProcess = Process.Start(ramPsi);
            var ramResult = await ramProcess?.StandardOutput.ReadToEndAsync()!;

            var uptimePsi = new ProcessStartInfo { FileName = "/bin/bash", Arguments = "-c \"uptime -p\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var uptimeProcess = Process.Start(uptimePsi);
            var uptimeResult = (await uptimeProcess?.StandardOutput.ReadToEndAsync()!).Trim().Replace("up ", "");

            var diskPsi = new ProcessStartInfo { FileName = "/bin/bash", Arguments = "-c \"df -h / | awk 'NR==2 {print $5}'\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var diskProcess = Process.Start(diskPsi);
            var diskResult = (await diskProcess?.StandardOutput.ReadToEndAsync()!).Trim().Replace("%", "");

            if (!string.IsNullOrEmpty(ramResult))
            {
                try
                {
                    var lines = ramResult.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    var parts = lines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    var stats = new SystemStats
                    {
                        MachineName = Environment.MachineName, 
                        TotalRam = parts[1],
                        UsedRam = parts[2],
                        CpuUsage = cpuUsage.ToString("F1"),
                        Uptime = uptimeResult ?? "Unknown",
                        DiskUsage = diskResult ?? "0",
                        CapturedAt = DateTime.UtcNow 
                    };

                    System.Console.Clear();
                    DisplayDashboard(stats);

                    try 
                    {
                       
                        var response = await client.PostAsJsonAsync("http://localhost:5005/api/metrics", stats);
                        
                        if (response.IsSuccessStatusCode)
                        {
                            System.Console.WriteLine("Data sent successfully!");
                        }
                        else
                        {
                            System.Console.WriteLine($"Server Error: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"Connection Error: {ex.Message}");
                    }
                }
                catch (Exception ex) { System.Console.WriteLine($"Error: {ex.Message}"); }
            }
            Thread.Sleep(1000); 
        }
    }

    private static void DisplayDashboard(SystemStats stats)
    {
        double total = double.Parse(stats.TotalRam);
        double used = double.Parse(stats.UsedRam);
        double ramPercentage = (used / total) * 100;
        double cpuPercentage = double.TryParse(stats.CpuUsage, out var c) ? c : 0;
        double diskPercentage = double.TryParse(stats.DiskUsage, out var d) ? d : 0;

        System.Console.WriteLine("======================================");
        System.Console.WriteLine("       SYSGUARD AGENT (ACTIVE)        ");
        System.Console.WriteLine("======================================");
        System.Console.WriteLine($"Uptime: {stats.Uptime} | {stats.CapturedAt:HH:mm:ss}");
        System.Console.WriteLine("--------------------------------------");
        
        System.Console.Write("CPU : "); ApplyColorCoding(cpuPercentage); System.Console.WriteLine($"{cpuPercentage:F1}%"); System.Console.ResetColor();
        System.Console.Write("RAM : "); ApplyColorCoding(ramPercentage); System.Console.WriteLine($"{stats.UsedRam} MB ({ramPercentage:F1}%)"); System.Console.ResetColor();
        System.Console.Write("DISK: "); ApplyColorCoding(diskPercentage); System.Console.WriteLine($"{diskPercentage}%"); System.Console.ResetColor();
        System.Console.WriteLine("======================================");
        System.Console.WriteLine($"Status: Sending data to API...");
    }

    private static void ApplyColorCoding(double percentage)
    {
        System.Console.ForegroundColor = percentage > 80 ? ConsoleColor.Red : (percentage > 50 ? ConsoleColor.Yellow : ConsoleColor.Green);
    }
}