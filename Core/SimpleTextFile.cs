using System.Text;

namespace Core;

public class SimpleTextFile
{
    private readonly string _path;

    public SimpleTextFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(path));
        }

        _path = path;
    }

    // Overwrite file with new content
    public void WriteLines(IEnumerable<string> lines)
    {
        try
        {
            CreateDirectoryIfNeeded();

            File.WriteAllLines(
                _path,
                lines,
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing file {_path}: {ex.Message}");
        }
    }

    // Read all lines
    public string[] ReadLines()
    {
        try
        {
            CreateFileIfNeeded();

            return File.ReadAllLines(
                _path,
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file {_path}: {ex.Message}");
            return Array.Empty<string>();
        }
    }

    // Append a single line
    public void AppendLine(string line)
    {
        try
        {
            CreateFileIfNeeded();

            File.AppendAllText(
                _path,
                line + Environment.NewLine,
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error appending file {_path}: {ex.Message}");
        }
    }

    // Append multiple lines
    public void AppendLines(IEnumerable<string> lines)
    {
        try
        {
            CreateFileIfNeeded();

            File.AppendAllLines(
                _path,
                lines,
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error appending file {_path}: {ex.Message}");
        }
    }

    // Verify file existence
    public bool Exists()
    {
        return File.Exists(_path);
    }

    // Delete file
    public void Delete()
    {
        try
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file {_path}: {ex.Message}");
        }
    }

    // Get file size in bytes
    public long GetFileSize()
    {
        try
        {
            if (File.Exists(_path))
            {
                return new FileInfo(_path).Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting file size {_path}: {ex.Message}");
        }

        return 0;
    }

    // Create directory if needed
    private void CreateDirectoryIfNeeded()
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrWhiteSpace(directory) &&
            !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    // Create file if needed
    private void CreateFileIfNeeded()
    {
        CreateDirectoryIfNeeded();

        if (!File.Exists(_path))
        {
            File.WriteAllText(
                _path,
                string.Empty,
                Encoding.UTF8);
        }
    }
}