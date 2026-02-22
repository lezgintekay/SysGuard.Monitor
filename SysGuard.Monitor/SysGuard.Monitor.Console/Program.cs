namespace SysGuard.Monitor.Console;
using System.Diagnostics;
using System ; 

class Program
{
    static void Main(string[] args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = "-c \"free -m\"",
            RedirectStandardOutput =  true,
            UseShellExecute = false,
            CreateNoWindow = true,
            
        };
        
        using var process = Process.Start(psi);

        if (process == null)
        {
            Console.WriteLine("Could not start SysGuard.Monitor.Console");
            return; 
        }
        using var reader = process.StandardOutput;
        string result = reader.ReadToEnd();

        if (string.IsNullOrWhiteSpace(result))
        {
            Console.WriteLine("SysGuard.Monitor.Console returned an empty string");
            return;
        }
        Console.WriteLine("--- System RAM Information ---");
        Console.WriteLine(result);
    }
}

