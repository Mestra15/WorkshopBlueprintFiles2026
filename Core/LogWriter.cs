using System.Text;

namespace Core;

public class LogWriter : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public LogWriter(string path)
    {
        try
        {
            string? directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrWhiteSpace(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _writer = new StreamWriter(
                path,
                append: true,
                encoding: Encoding.UTF8)
            {
                AutoFlush = true
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating log file: {ex.Message}");
            throw;
        }
    }

    // Full log entry
    public void WriteLog(string level, string user, string message)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string logEntry =
                $"{timestamp} | " +
                $"{level.ToUpper().PadRight(7)} | " +
                $"User: {user.PadRight(15)} | " +
                $"{message}";

            _writer.WriteLine(logEntry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing log: {ex.Message}");
        }
    }

    // System log entry
    public void WriteLog(string level, string message)
    {
        WriteLog(level, "SYSTEM", message);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _writer?.Close();
                _writer?.Dispose();
            }

            _disposed = true;
        }
    }
}