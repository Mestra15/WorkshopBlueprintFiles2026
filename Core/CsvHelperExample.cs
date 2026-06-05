using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Core;

public class CsvHelperExample
{
    // Write a list of Person objects to a CSV file (manual)
    public void Write(string path, IEnumerable<Person> people)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var sw = new StreamWriter(path);

            // Write header
            sw.WriteLine("Id,FirstName,LastName,Phone,City,Balance");

            // Write each person
            foreach (var person in people)
            {
                sw.WriteLine($"{person.Id},{EscapeCsv(person.FirstName)},{EscapeCsv(person.LastName)},{EscapeCsv(person.Phone)},{EscapeCsv(person.City)},{person.Balance}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing CSV file: {ex.Message}");
        }
    }

    // Read a list of Person objects from a CSV file (manual)
    public List<Person> Read(string path)
    {
        var result = new List<Person>();

        try
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"File not found: {path}");
                return result;
            }

            using var sr = new StreamReader(path);
            string? line;
            bool isFirstLine = true;

            while ((line = sr.ReadLine()) != null)
            {
                if (isFirstLine)
                {
                    isFirstLine = false;
                    continue; // Skip header
                }

                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length >= 6)
                {
                    result.Add(new Person
                    {
                        Id = int.Parse(parts[0]),
                        FirstName = UnescapeCsv(parts[1]),
                        LastName = UnescapeCsv(parts[2]),
                        Phone = UnescapeCsv(parts[3]),
                        City = UnescapeCsv(parts[4]),
                        Balance = decimal.Parse(parts[5])
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
        }

        return result;
    }

    // Append a list of Person objects to a CSV file
    public void Append(string path, IEnumerable<Person> people)
    {
        try
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var sw = new StreamWriter(path, append: true);

            foreach (var person in people)
            {
                sw.WriteLine($"{person.Id},{EscapeCsv(person.FirstName)},{EscapeCsv(person.LastName)},{EscapeCsv(person.Phone)},{EscapeCsv(person.City)},{person.Balance}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error appending to CSV file: {ex.Message}");
        }
    }

    // Helper method to escape CSV fields
    private string EscapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    // Helper method to unescape CSV fields
    private string UnescapeCsv(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;

        field = field.Trim();
        if (field.StartsWith("\"") && field.EndsWith("\""))
        {
            field = field.Substring(1, field.Length - 2);
            field = field.Replace("\"\"", "\"");
        }

        return field;
    }
}