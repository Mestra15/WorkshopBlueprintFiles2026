using System.Text;
using System.Text.RegularExpressions;

namespace Core;

public class Program
{
    // ====================================================================
    // GLOBAL VARIABLES
    // ====================================================================
    private static List<Person> _persons = new List<Person>();
    private static List<(string Username, string Password, bool IsActive)> _users = new List<(string, string, bool)>();
    private static string _currentUser = "";

    // Use absolute paths based on the executable location
    private static string _basePath = AppDomain.CurrentDomain.BaseDirectory;
    private static string _usersFile = Path.Combine(_basePath, "Users.txt");
    private static string _personsFile = Path.Combine(_basePath, "personas.txt");
    private static string _logFile = Path.Combine(_basePath, "log.txt");

    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        // Load data on startup
        LoadUsers();
        LoadPersons();
        CreateSampleDataIfEmpty();

        // ====================================================================
        // MAIN MENU - FLAT FILES WORKSHOP
        // ====================================================================

        if (!Authenticate())
        {
            Console.WriteLine("\nAccess denied. Contact administrator.");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.WriteLine($"Welcome {_currentUser}!");
        WriteLog("LOGIN_SUCCESS", "User authenticated successfully");

        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("        FLAT FILES WORKSHOP");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Person CRUD System");
            Console.WriteLine("0. Exit");
            Console.WriteLine("========================================");
            Console.Write("Choose an option: ");

            string option = Console.ReadLine() ?? "";

            switch (option)
            {
                case "1":
                    RunPersonCRUD();
                    break;
                case "0":
                    exit = true;
                    WriteLog("LOGOUT", "User logged out");
                    SavePersons();
                    Console.WriteLine("\nGoodbye!");
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }

    // ====================================================================
    // SAMPLE DATA CREATION
    // ====================================================================

    private static void CreateSampleDataIfEmpty()
    {
        try
        {
            if (!File.Exists(_personsFile) || File.ReadAllLines(_personsFile).Length == 0)
            {
                var samplePersons = new List<Person>
                {
                    new() { Id = 1, FirstName = "Carlos", LastName = "Gomez", Phone = "3101112222", City = "Bogota", Balance = 2500000.00m },
                    new() { Id = 2, FirstName = "Ana", LastName = "Lopez", Phone = "3103334444", City = "Medellin", Balance = 1800000.00m },
                    new() { Id = 3, FirstName = "Luis", LastName = "Martinez", Phone = "3105556666", City = "Cali", Balance = 3200000.00m }
                };

                var lines = samplePersons.Select(p => p.ToString()).ToArray();
                File.WriteAllLines(_personsFile, lines);
                WriteLog("SAMPLE_DATA", $"Created {samplePersons.Count} sample records");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating sample data: {ex.Message}");
        }
    }

    // ====================================================================
    // AUTHENTICATION METHODS
    // ====================================================================

    private static void LoadUsers()
    {
        try
        {
            if (!File.Exists(_usersFile))
            {
                File.WriteAllLines(_usersFile, new[]
                {
                    "carlos,Carlos123!,true",
                    "ana,Ana456*,true",
                    "luis,Luis789#,true"
                });
            }

            var lines = File.ReadAllLines(_usersFile);
            _users.Clear();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var parts = line.Split(',');
                    _users.Add((parts[0], parts[1], bool.Parse(parts[2])));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading users: {ex.Message}");
        }
    }

    private static void SaveUsers()
    {
        try
        {
            var lines = _users.Select(u => $"{u.Username},{u.Password},{u.IsActive}").ToArray();
            File.WriteAllLines(_usersFile, lines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving users: {ex.Message}");
        }
    }

    private static bool Authenticate()
    {
        int attempts = 0;

        while (attempts < 3)
        {
            Console.Clear();
            Console.WriteLine("=== LOGIN SYSTEM ===\n");
            Console.Write("Username: ");
            string username = Console.ReadLine() ?? "";
            Console.Write("Password: ");
            string password = ReadPassword();

            var user = _users.FirstOrDefault(u => u.Username == username);

            if (user.Username == null)
            {
                Console.WriteLine("\nUser not found");
                WriteLog("LOGIN_FAILED", $"User not found: {username}");
                attempts++;
                Console.ReadKey();
                continue;
            }

            if (!user.IsActive)
            {
                Console.WriteLine("\nUser is blocked. Contact administrator.");
                WriteLog("LOGIN_BLOCKED", $"Blocked user attempted login: {username}");
                Console.ReadKey();
                return false;
            }

            if (user.Password == password)
            {
                _currentUser = username;
                return true;
            }

            attempts++;
            Console.WriteLine($"\nInvalid password. Attempts left: {3 - attempts}");
            WriteLog("LOGIN_FAILED", $"Invalid password for: {username} (Attempt {attempts}/3)");

            if (attempts >= 3)
            {
                var index = _users.FindIndex(u => u.Username == username);
                if (index >= 0)
                {
                    var updatedUser = _users[index];
                    updatedUser.IsActive = false;
                    _users[index] = updatedUser;
                    SaveUsers();
                }
                WriteLog("USER_BLOCKED", $"User blocked after 3 failed attempts: {username}");
                Console.WriteLine("\nUser has been blocked after 3 failed attempts");
            }

            Console.ReadKey();
        }

        return false;
    }

    private static string ReadPassword()
    {
        string password = "";
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                password += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[0..^1];
                Console.Write("\b \b");
            }
        } while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return password;
    }

    private static void UnlockUser()
    {
        Console.Clear();
        Console.WriteLine("=== UNLOCK USER ===\n");

        Console.Write("Enter username to unlock: ");
        string username = Console.ReadLine() ?? "";

        var index = _users.FindIndex(u => u.Username == username);
        if (index >= 0 && !_users[index].IsActive)
        {
            var updatedUser = _users[index];
            updatedUser.IsActive = true;
            _users[index] = updatedUser;
            SaveUsers();
            WriteLog("USER_UNLOCKED", $"User unlocked: {username}");
            Console.WriteLine($"\nUser {username} has been unlocked successfully!");
        }
        else if (index >= 0 && _users[index].IsActive)
        {
            Console.WriteLine($"\nUser {username} is already active.");
        }
        else
        {
            Console.WriteLine($"\nUser {username} not found.");
        }

        Console.ReadKey();
    }

    // ====================================================================
    // LOGGING METHODS
    // ====================================================================

    private static void WriteLog(string operation, string details)
    {
        try
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {_currentUser} | Operation: {operation} | Details: {details}";
            File.AppendAllText(_logFile, logEntry + Environment.NewLine, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing log: {ex.Message}");
        }
    }

    // ====================================================================
    // PERSON CRUD METHODS
    // ====================================================================

    private static void LoadPersons()
    {
        try
        {
            if (!File.Exists(_personsFile))
            {
                File.WriteAllText(_personsFile, "");
                return;
            }

            var lines = File.ReadAllLines(_personsFile);
            _persons.Clear();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    _persons.Add(Person.FromString(line));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading persons: {ex.Message}");
        }
    }

    private static void SavePersons()
    {
        try
        {
            var lines = _persons.Select(p => p.ToString()).ToArray();
            File.WriteAllLines(_personsFile, lines);
            WriteLog("DATA_SAVED", $"Saved {_persons.Count} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving persons: {ex.Message}");
        }
    }

    private static bool ValidateId(int id)
    {
        return id > 0 && !_persons.Any(p => p.Id == id);
    }

    private static bool ValidateName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               Regex.IsMatch(name, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]{2,50}$");
    }

    private static bool ValidatePhone(string phone)
    {
        return Regex.IsMatch(phone, @"^\d{7,15}$");
    }

    private static bool ValidateBalance(decimal balance)
    {
        return balance >= 0;
    }

    private static void AddPerson()
    {
        Console.Clear();
        Console.WriteLine("=== ADD NEW PERSON ===\n");

        int id;
        do
        {
            Console.Write("ID (positive unique number): ");
            if (!int.TryParse(Console.ReadLine(), out id) || !ValidateId(id))
            {
                Console.WriteLine("Error: Invalid ID or ID already exists. Try again.");
                WriteLog("ADD_PERSON_ERROR", $"Invalid or duplicate ID: {id}");
            }
            else break;
        } while (true);

        string firstName;
        do
        {
            Console.Write("First Name (letters only, 2-50 chars): ");
            firstName = Console.ReadLine() ?? "";
            if (!ValidateName(firstName))
            {
                Console.WriteLine("Error: Invalid first name. Only letters, minimum 2 characters.");
                WriteLog("ADD_PERSON_ERROR", $"Invalid first name: {firstName}");
            }
            else break;
        } while (true);

        string lastName;
        do
        {
            Console.Write("Last Name (letters only, 2-50 chars): ");
            lastName = Console.ReadLine() ?? "";
            if (!ValidateName(lastName))
            {
                Console.WriteLine("Error: Invalid last name. Only letters, minimum 2 characters.");
                WriteLog("ADD_PERSON_ERROR", $"Invalid last name: {lastName}");
            }
            else break;
        } while (true);

        string phone;
        do
        {
            Console.Write("Phone (7-15 digits): ");
            phone = Console.ReadLine() ?? "";
            if (!ValidatePhone(phone))
            {
                Console.WriteLine("Error: Invalid phone. Only numbers, 7-15 digits.");
                WriteLog("ADD_PERSON_ERROR", $"Invalid phone: {phone}");
            }
            else break;
        } while (true);

        Console.Write("City: ");
        string city = Console.ReadLine() ?? "";

        decimal balance;
        do
        {
            Console.Write("Balance (positive number): ");
            if (!decimal.TryParse(Console.ReadLine(), out balance) || !ValidateBalance(balance))
            {
                Console.WriteLine("Error: Invalid balance. Must be a positive number.");
                WriteLog("ADD_PERSON_ERROR", $"Invalid balance: {balance}");
            }
            else break;
        } while (true);

        var person = new Person
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            City = city,
            Balance = balance
        };

        _persons.Add(person);
        SavePersons();

        WriteLog("ADD_PERSON", $"Added person ID:{id} - {firstName} {lastName}");
        Console.WriteLine("\nPerson added successfully!");
        Console.ReadKey();
    }

    private static void EditPerson()
    {
        Console.Clear();
        Console.WriteLine("=== EDIT PERSON ===\n");

        Console.Write("Enter person ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            WriteLog("EDIT_PERSON_ERROR", "Invalid ID entered");
            Console.ReadKey();
            return;
        }

        var person = _persons.FirstOrDefault(p => p.Id == id);
        if (person == null)
        {
            Console.WriteLine("Person not found.");
            WriteLog("EDIT_PERSON_ERROR", $"Person ID:{id} not found");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"\nEditing: {person.FullName}");
        Console.WriteLine("(Press ENTER to keep current value)\n");

        Console.Write($"Current First Name ({person.FirstName}): ");
        string input = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (ValidateName(input))
                person.FirstName = input;
            else
                Console.WriteLine("Invalid name, keeping original.");
        }

        Console.Write($"Current Last Name ({person.LastName}): ");
        input = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (ValidateName(input))
                person.LastName = input;
            else
                Console.WriteLine("Invalid last name, keeping original.");
        }

        Console.Write($"Current Phone ({person.Phone}): ");
        input = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(input))
        {
            if (ValidatePhone(input))
                person.Phone = input;
            else
                Console.WriteLine("Invalid phone, keeping original.");
        }

        Console.Write($"Current City ({person.City}): ");
        input = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(input))
            person.City = input;

        Console.Write($"Current Balance ({person.Balance:C}): ");
        input = Console.ReadLine() ?? "";
        if (!string.IsNullOrWhiteSpace(input) && decimal.TryParse(input, out decimal newBalance))
        {
            if (ValidateBalance(newBalance))
                person.Balance = newBalance;
            else
                Console.WriteLine("Invalid balance, keeping original.");
        }

        SavePersons();
        WriteLog("EDIT_PERSON", $"Edited person ID:{id} - {person.FullName}");
        Console.WriteLine("\nPerson updated successfully!");
        Console.ReadKey();
    }

    private static void DeletePerson()
    {
        Console.Clear();
        Console.WriteLine("=== DELETE PERSON ===\n");

        Console.Write("Enter person ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("Invalid ID.");
            WriteLog("DELETE_PERSON_ERROR", "Invalid ID entered");
            Console.ReadKey();
            return;
        }

        var person = _persons.FirstOrDefault(p => p.Id == id);
        if (person == null)
        {
            Console.WriteLine("Person not found.");
            WriteLog("DELETE_PERSON_ERROR", $"Person ID:{id} not found");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"\nPerson data to delete:");
        Console.WriteLine($"ID: {person.Id}");
        Console.WriteLine($"Name: {person.FullName}");
        Console.WriteLine($"Phone: {person.Phone}");
        Console.WriteLine($"City: {person.City}");
        Console.WriteLine($"Balance: {person.Balance:C}");

        Console.Write("\nAre you sure you want to delete this person? (Y/N): ");
        if (Console.ReadLine()?.ToUpper() == "Y")
        {
            _persons.Remove(person);
            SavePersons();
            WriteLog("DELETE_PERSON", $"Deleted person ID:{id} - {person.FullName}");
            Console.WriteLine("\nPerson deleted successfully!");
        }
        else
        {
            Console.WriteLine("\nOperation cancelled.");
            WriteLog("DELETE_PERSON_CANCELLED", $"Deletion cancelled for person ID:{id}");
        }

        Console.ReadKey();
    }

    private static void ShowAllPersons()
    {
        Console.Clear();
        Console.WriteLine("=== ALL PERSONS ===\n");

        if (_persons.Count == 0)
        {
            Console.WriteLine("No persons registered.");
        }
        else
        {
            Console.WriteLine($"{"ID",-5} {"First Name",-20} {"Last Name",-20} {"Phone",-15} {"City",-15} {"Balance",15}");
            Console.WriteLine(new string('-', 95));

            foreach (var person in _persons)
            {
                Console.WriteLine($"{person.Id,-5} {person.FirstName,-20} {person.LastName,-20} {person.Phone,-15} {person.City,-15} {person.Balance,15:F2}");
            }
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    // ====================================================================
    // REPORT BY CITY - EXACT FORMAT AS REQUESTED
    // ====================================================================
    private static void ShowReportByCity()
    {
        Console.Clear();
        Console.WriteLine("=== BALANCE REPORT BY CITY ===\n");

        var cities = _persons.Where(p => !string.IsNullOrWhiteSpace(p.City))
                           .GroupBy(p => p.City)
                           .OrderBy(g => g.Key);

        if (!cities.Any())
        {
            Console.WriteLine("No data available.");
            Console.ReadKey();
            return;
        }

        decimal grandTotal = 0;

        foreach (var city in cities)
        {
            Console.WriteLine($"\nCity: {city.Key}\n");
            Console.WriteLine($"{"ID",-5} {"First Name",-20} {"Last Name",-20} {"Balance",15}");
            Console.WriteLine(new string('-', 65));

            decimal cityTotal = 0;
            foreach (var person in city)
            {
                Console.WriteLine($"{person.Id,-5} {person.FirstName,-20} {person.LastName,-20} {person.Balance,15:F2}");
                cityTotal += person.Balance;
            }

            Console.WriteLine(new string('-', 65));
            Console.WriteLine($"{"Total: " + city.Key + ":",-45} {cityTotal,15:F2}");
            Console.WriteLine(new string('=', 65));

            grandTotal += cityTotal;
        }

        Console.WriteLine($"\n{"Grand Total:",-45} {grandTotal,15:F2}");
        Console.WriteLine(new string('=', 65));

        WriteLog("REPORT_GENERATED", $"Generated city report. Grand total: {grandTotal:F2}");
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static void RunPersonCRUD()
    {
        bool exit = false;

        while (!exit)
        {
            Console.Clear();
            Console.WriteLine("=== PERSON CRUD SYSTEM ===");
            Console.WriteLine("1. Show all persons");
            Console.WriteLine("2. Add person");
            Console.WriteLine("3. Edit person");
            Console.WriteLine("4. Delete person");
            Console.WriteLine("5. Show report by city");
            Console.WriteLine("6. Unlock user");
            Console.WriteLine("0. Back to main menu");
            Console.WriteLine("========================================");
            Console.Write("Choose an option: ");

            string option = Console.ReadLine() ?? "";

            switch (option)
            {
                case "1":
                    ShowAllPersons();
                    break;
                case "2":
                    AddPerson();
                    break;
                case "3":
                    EditPerson();
                    break;
                case "4":
                    DeletePerson();
                    break;
                case "5":
                    ShowReportByCity();
                    break;
                case "6":
                    UnlockUser();
                    break;
                case "0":
                    exit = true;
                    break;
                default:
                    Console.WriteLine("Invalid option. Press any key to continue...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}