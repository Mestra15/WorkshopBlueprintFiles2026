using System.Text;

namespace Core;

public class FixedWidthExample
{
    // Write records to fixed-width file
    public void Write(string path, IEnumerable<FixedWidthRecord> records)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid file path.");
            }

            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllLines(
                path,
                records.Select(r => r.ToString()),
                Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing fixed-width file: {ex.Message}");
        }
    }

    // Read records from fixed-width file
    public List<FixedWidthRecord> Read(string path)
    {
        var records = new List<FixedWidthRecord>();

        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return records;
            }

            var lines = File.ReadAllLines(path, Encoding.UTF8);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    records.Add(FixedWidthRecord.Parse(line));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Invalid record skipped: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading fixed-width file: {ex.Message}");
        }

        return records;
    }
}