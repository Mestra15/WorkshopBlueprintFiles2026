using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core;

public class ManualCsvHelper
{
    // Write CSV file from string arrays
    public void WriteCSV(string path, List<string[]> records)
    {
        try
        {
            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var sw = new StreamWriter(path);

            foreach (var fields in records)
            {
                // Escape fields that contain commas, quotes, or newlines
                var escapedFields = fields.Select(f => EscapeCsvField(f)).ToArray();
                var line = string.Join(",", escapedFields);
                sw.WriteLine(line);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing CSV file: {ex.Message}");
        }
    }

    // Read CSV file into string arrays
    public List<string[]> ReadCSV(string path)
    {
        var result = new List<string[]>();

        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return result;
            }

            using var sr = new StreamReader(path);
            string? line;

            while ((line = sr.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var fields = ParseCsvLine(line);
                result.Add(fields);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }

        return result;
    }

    // Escape a CSV field (handles commas, quotes, and newlines)
    private string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        bool needsQuoting = field.Contains(",") ||
                           field.Contains("\"") ||
                           field.Contains("\n") ||
                           field.Contains("\r");

        if (needsQuoting)
        {
            // Double up any quotes
            string escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }

        return field;
    }

    // Parse a CSV line (handles quoted fields)
    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        int startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                string field = line.Substring(startIndex, i - startIndex);
                result.Add(UnescapeCsvField(field));
                startIndex = i + 1;
            }
        }

        // Add the last field
        string lastField = line.Substring(startIndex);
        result.Add(UnescapeCsvField(lastField));

        return result.ToArray();
    }

    // Remove quotes and unescape double quotes
    private string UnescapeCsvField(string field)
    {
        field = field.Trim();

        if (field.StartsWith("\"") && field.EndsWith("\""))
        {
            field = field.Substring(1, field.Length - 2);
            field = field.Replace("\"\"", "\"");
        }

        return field;
    }
}