using System;
using System.IO;

namespace PrisciluOrigins;

public static class PrisciluLogger
{
    private static string? _logPath;
    private static bool _initialized = false;

    public static void Init(string modPath)
    {
        _logPath = Path.Combine(modPath, "debug.log");
        try 
        {
            // Overwrite existing file (Create)
            File.WriteAllText(_logPath, $"[PrisciluOrigins] Debug Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n------------------------------------------------\n");
            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PrisciluOrigins] Failed to initialize log file: {ex.Message}");
        }
    }

    public static void Log(string message)
    {
        if (!_initialized || _logPath == null) return;
        try
        {
             File.AppendAllText(_logPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
        }
        catch 
        {
            // Fail silently to avoid console spam
        }
    }
}
