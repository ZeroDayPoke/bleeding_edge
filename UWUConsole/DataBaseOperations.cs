// DatabaseOperations.cs

using Microsoft.EntityFrameworkCore;
using System.Reflection;

public interface IDatabaseOperations
{
    void Create(string[] arguments);
    void Show(string[] arguments);
    void Destroy(string[] arguments);
    void All(string[] arguments);
    void Update(string[] arguments);
}

public class DatabaseOperations : IDatabaseOperations
{
    private readonly MyDbContext _context;
    private readonly IEmailService _emailService;
    private readonly Dictionary<string, Type> _classDictionary;

    public DatabaseOperations(MyDbContext context, IEmailService emailService, Dictionary<string, Type> classDictionary)
    {
        _context = context;
        _emailService = emailService;
        _classDictionary = classDictionary;
    }

    public void Create(string[] arguments)
    {
        if (arguments.Length < 2)
        {
            Console.WriteLine("Usage: create <class> <attribute1=value1> <attribute2=value2> ...");
            return;
        }

        var className = arguments[0];
        if (!_classDictionary.TryGetValue(className, out var classType))
        {
            Console.WriteLine($"Invalid class. Available classes: {string.Join(", ", _classDictionary.Keys)}");
            return;
        }

        var instance = Activator.CreateInstance(classType);
        for (int i = 1; i < arguments.Length; i++)
        {
            var pair = arguments[i].Split('=');
            if (pair.Length != 2)
            {
                Console.WriteLine($"Invalid attribute format: {arguments[i]}");
                return;
            }

            if (pair[0] == "Password" && instance is User user)
            {
                user.SetPassword(pair[1]);
                continue;
            }

            var property = classType.GetProperty(pair[0]);
            if (property == null)
            {
                Console.WriteLine($"Invalid attribute: {pair[0]}");
                return;
            }

            object value;
            if (property.PropertyType.IsEnum)
            {
                value = Enum.Parse(property.PropertyType, pair[1]);
            }
            else
            {
                value = Convert.ChangeType(pair[1], property.PropertyType);
            }

            Console.WriteLine($"Setting property {pair[0]} to value {pair[1]}");
            property.SetValue(instance, value);
        }

        try
        {
            if (instance != null)
            {
                _context.Add(instance);
                _context.SaveChanges();

                // If a User was created, send the verification email
                if (instance is User user)
                    try
                    {
                        _emailService.SendVerificationEmail(user.Email, user.VerificationToken);
                        Console.WriteLine($"Verification email sent to {user.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send verification email: {ex.Message}");
                    }
                Console.WriteLine($"{className} created with ID {((dynamic)instance).Id}");
            }
            else
            {
                Console.WriteLine($"Failed to create {className} instance.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving changes: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    public void Show(string[] arguments)
    {
        if (arguments.Length != 2)
        {
            Console.WriteLine("Usage: show <class> <id>");
            return;
        }

        var className = arguments[0];
        if (!int.TryParse(arguments[1], out var id))
        {
            Console.WriteLine("Invalid ID. ID must be an integer.");
            return;
        }

        var classType = _classDictionary[className];
        var instance = _context.Find(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        Console.WriteLine(instance);
    }

    public void Destroy(string[] arguments)
    {
        if (arguments.Length != 2)
        {
            Console.WriteLine("Usage: destroy <class> <id>");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = _classDictionary[className];
        var instance = _context.Find(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        _context.Remove(instance);
        _context.SaveChanges();

        Console.WriteLine($"{className} with ID {id} has been deleted.");
    }

    public void All(string[] arguments)
    {
        if (arguments.Length != 1)
        {
            Console.WriteLine("Usage: all <class>");
            return;
        }

        var className = arguments[0];
        if (!_classDictionary.TryGetValue(className, out var classType))
        {
            Console.WriteLine($"Invalid class. Available classes: {string.Join(", ", _classDictionary.Keys)}");
            return;
        }

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
        if (method == null)
        {
            Console.WriteLine("Unable to get the Set method from DbContext.");
            return;
        }

        var genericMethod = method.MakeGenericMethod(classType);
        var set = genericMethod.Invoke(_context, null) as IEnumerable<object>;
        if (set == null)
        {
            Console.WriteLine($"Unable to get entities of type {className}.");
            return;
        }

        foreach (var instance in set)
        {
            Console.WriteLine(instance);
        }
    }

    public void Update(string[] arguments)
    {
        if (arguments.Length < 3)
        {
            Console.WriteLine("Usage: update <class> <id> <attribute1=value1> <attribute2=value2> ...");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = _classDictionary[className];
        var instance = _context.Find(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        for (int i = 2; i < arguments.Length; i++)
        {
            var pair = arguments[i].Split('=');
            if (pair.Length != 2)
            {
                Console.WriteLine($"Invalid attribute format: {arguments[i]}");
                return;
            }

            var property = classType.GetProperty(pair[0]);
            if (property == null)
            {
                Console.WriteLine($"Invalid attribute: {pair[0]}");
                return;
            }

            property.SetValue(instance, Convert.ChangeType(pair[1], property.PropertyType));
        }

        _context.Update(instance);
        _context.SaveChanges();

        Console.WriteLine($"{className} with ID {id} has been updated.");
    }
}
