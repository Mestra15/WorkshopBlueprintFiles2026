namespace Core;

public class FixedWidthRecord
{
    // Product code (5 characters)
    public string Code { get; set; } = string.Empty;

    // Product description (40 characters)
    public string Description { get; set; } = string.Empty;

    // Product category (15 characters)
    public string Category { get; set; } = string.Empty;

    // Quantity in stock (5 characters)
    public int Quantity { get; set; }

    // Unit price (12 characters)
    public decimal Price { get; set; }

    // Calculated value
    public decimal Value => Quantity * Price;

    // Convert object to fixed-width string
    public override string ToString()
    {
        return string.Concat(
            Code.PadRight(5),
            Description.PadRight(40),
            Category.PadRight(15),
            Quantity.ToString().PadLeft(5),
            Price.ToString("F2").PadLeft(12)
        );
    }

    // Convert fixed-width string to object
    public static FixedWidthRecord Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new ArgumentException("Record cannot be empty.");
        }

        if (line.Length < 77)
        {
            throw new ArgumentException(
                $"Invalid record length. Expected at least 77 characters, found {line.Length}."
            );
        }

        return new FixedWidthRecord
        {
            Code = line.Substring(0, 5).Trim(),
            Description = line.Substring(5, 40).Trim(),
            Category = line.Substring(45, 15).Trim(),
            Quantity = int.TryParse(line.Substring(60, 5).Trim(), out int qty)
                ? qty
                : 0,
            Price = decimal.TryParse(line.Substring(65, 12).Trim(), out decimal price)
                ? price
                : 0
        };
    }
}