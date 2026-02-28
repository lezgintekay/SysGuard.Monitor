namespace SysGuard.Monitor.Console.Models;

public class SystemStats
{
    public string TotalRam { get; set; } = string.Empty;
    public string UsedRam { get; set; } = string.Empty;
    public string CpuUsage { get; set; } =  string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public string DiskUsage { get; set; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.Now;
}