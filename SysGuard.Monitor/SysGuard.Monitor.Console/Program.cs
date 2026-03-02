namespace SysGuard.Monitor.Console;

using System.Diagnostics;
using System;
using System.IO; 
using System.Threading; 
using SysGuard.Monitor.Models; // Doğru namespace
using System.Linq;
using System.Net.Http.Json; // JSON gönderimi için gerekli

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

    // Main metodunu 'async' yaptık ki API'ye veri gönderirken uygulama donmasın
    private static async Task Main()
    {
        Console.WriteLine("Starting SysGuard Agent...");
        using var client = new HttpClient(); // HTTP istemcisi oluşturuldu

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
            var ramResult = ramProcess?.StandardOutput.ReadToEnd();

            var uptimePsi = new ProcessStartInfo { FileName = "/bin/bash", Arguments = "-c \"uptime -p\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var uptimeProcess = Process.Start(uptimePsi);
            var uptimeResult = uptimeProcess?.StandardOutput.ReadToEnd().Trim().Replace("up ", "");

            var diskPsi = new ProcessStartInfo { FileName = "/bin/bash", Arguments = "-c \"df -h / | awk 'NR==2 {print $5}'\"", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
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
                        Uptime = uptimeResult ?? "Unknown",
                        DiskUsage = diskResult ?? "0",
                        CapturedAt = DateTime.Now
                    };

                    Console.Clear();
                    DisplayDashboard(stats);

                    // --- API'YE VERİ GÖNDERME ---
                    try 
                    {
                        await client.PostAsJsonAsync("http://localhost:5005/stats", stats);
                    }
                    catch { /* API kapalıysa sessizce devam et */ }
                }
                catch (Exception ex) { Console.WriteLine($"Error: {ex.Message}"); }
            }
            Thread.Sleep(500); 
        }
    }

    private static void DisplayDashboard(SystemStats stats)
    {
        double total = double.Parse(stats.TotalRam);
        double used = double.Parse(stats.UsedRam);
        double ramPercentage = (used / total) * 100;
        double cpuPercentage = double.TryParse(stats.CpuUsage, out var c) ? c : 0;
        double diskPercentage = double.TryParse(stats.DiskUsage, out var d) ? d : 0;

        Console.WriteLine("======================================");
        Console.WriteLine("       SYSGUARD AGENT (ACTIVE)        ");
        Console.WriteLine("======================================");
        Console.WriteLine($"Uptime: {stats.Uptime} | {stats.CapturedAt:HH:mm:ss}");
        Console.WriteLine("--------------------------------------");
        
        Console.Write("CPU : "); ApplyColorCoding(cpuPercentage); Console.WriteLine($"{cpuPercentage:F1}%"); Console.ResetColor();
        Console.Write("RAM : "); ApplyColorCoding(ramPercentage); Console.WriteLine($"{stats.UsedRam} MB ({ramPercentage:F1}%)"); Console.ResetColor();
        Console.Write("DISK: "); ApplyColorCoding(diskPercentage); Console.WriteLine($"{diskPercentage}%"); Console.ResetColor();
        Console.WriteLine("======================================");
        Console.WriteLine("Sending data to: http://localhost:5005");
    }

    private static void ApplyColorCoding(double percentage)
    {
        Console.ForegroundColor = percentage > 80 ? ConsoleColor.Red : (percentage > 50 ? ConsoleColor.Yellow : ConsoleColor.Green);
    }
}