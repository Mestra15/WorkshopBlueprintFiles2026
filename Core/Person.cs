namespace Core;

public class Person
{
    // Unique identifier
    public int Id { get; set; }

    // First name
    public string FirstName { get; set; } = string.Empty;

    // Last name
    public string LastName { get; set; } = string.Empty;

    // Phone number
    public string Phone { get; set; } = string.Empty;

    // City
    public string City { get; set; } = string.Empty;

    // Account balance
    public decimal Balance { get; set; }

    // Full name (read-only)
    public string FullName => $"{FirstName} {LastName}";

    // Convert Person to CSV string format
    public override string ToString()
    {
        return $"{Id},{FirstName},{LastName},{Phone},{City},{Balance}";
    }

    // Create Person from CSV string
    public static Person FromString(string line)
    {
        var parts = line.Split(',');

        if (parts.Length != 6)
        {
            throw new ArgumentException("Invalid person record format. Expected 6 fields separated by commas.");
        }

        return new Person
        {
            Id = int.Parse(parts[0]),
            FirstName = parts[1],
            LastName = parts[2],
            Phone = parts[3],
            City = parts[4],
            Balance = decimal.Parse(parts[5])
        };
    }
}