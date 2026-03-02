namespace SysGuard.Monitor.Models;
public class SystemStats
{
    public string TotalRam { get; init; } = string.Empty;
    public string UsedRam { get; init; } = string.Empty;
    public string CpuUsage { get; init; } =  string.Empty;
    public string Uptime { get; init; } = string.Empty;
    public string DiskUsage { get; init; } = string.Empty;
    public DateTime CapturedAt { get; set; } = DateTime.Now;
}