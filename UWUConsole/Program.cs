using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Security.Cryptography;

public class Program
{
    private static Dictionary<string, Type> classDictionary = new Dictionary<string, Type>
    {
        { "User", typeof(User) },
        { "Role", typeof(Role) }
    };

    public static void Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build());

        var configuration = services.BuildServiceProvider().GetService<IConfiguration>();

        if (configuration != null)
        {
            services.AddDbContext<MyDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 33))));
        }
        else
        {
            Console.WriteLine("Configuration is null. Unable to get the connection string.");
            return;
        }

        services.AddTransient<IEmailService, EmailService>();

        var provider = services.BuildServiceProvider();

        using var context = provider.GetRequiredService<MyDbContext>();
        var emailService = provider.GetRequiredService<IEmailService>();

        // Test the database connection
        if (!context.Database.CanConnect())
        {
            Console.WriteLine("Cannot connect to database");
            return;
        }
        Console.WriteLine("Link established");

        // Command-line interface
        while (true)
        {
            Console.Write("(UWUSER Console) ");
            var input = Console.ReadLine();
            var command = "";
            if (input != null)
            {
                command = input.Split(' ')[0];
            }
            else
            {
                Console.WriteLine("Input is null. Please provide valid input.");
                return;
            }
            var arguments = input.Substring(command.Length).Trim().Split(' ');

            switch (command)
            {
                case "create":
                    Create(context, arguments, emailService);
                    break;
                case "show":
                    Show(context, arguments);
                    break;
                case "destroy":
                    Destroy(context, arguments);
                    break;
                case "all":
                    All(context, arguments);
                    break;
                case "update":
                    Update(context, arguments);
                    break;
                case "quit":
                    return;
                default:
                    Console.WriteLine($"Unknown command: {command}");
                    break;
            }
        }
    }


    private static void Create(MyDbContext context, string[] arguments, IEmailService emailService)
    {
        if (arguments.Length < 2)
        {
            Console.WriteLine("Usage: create <class> <attribute1=value1> <attribute2=value2> ...");
            return;
        }

        var className = arguments[0];
        if (!classDictionary.TryGetValue(className, out var classType))
        {
            Console.WriteLine($"Invalid class. Available classes: {string.Join(", ", classDictionary.Keys)}");
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

            Console.WriteLine($"Setting property {pair[0]} to value {pair[1]}");
            property.SetValue(instance, Convert.ChangeType(pair[1], property.PropertyType));
        }

        try
        {
            if (instance != null)
            {
                context.Add(instance);
                context.SaveChanges();

                // If a User was created, send the verification email
                if (instance is User user)
                    try
                    {
                        emailService.SendVerificationEmail(user.Email, user.VerificationToken);
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

    private static string GenerateSalt()
    {
        byte[] saltBytes = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return Convert.ToBase64String(saltBytes);
    }

    private static string HashPassword(string password, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, salt);
    }


    private static void Show(MyDbContext context, string[] arguments)
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

        var classType = classDictionary[className];
        var instance = context.Find(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        Console.WriteLine(instance);
    }

    private static void Destroy(MyDbContext context, string[] arguments)
    {
        if (arguments.Length != 2)
        {
            Console.WriteLine("Usage: destroy <class> <id>");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = classDictionary[className];
        var instance = context.Find(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        context.Remove(instance);
        context.SaveChanges();

        Console.WriteLine($"{className} with ID {id} has been deleted.");
    }

    private static void All(MyDbContext context, string[] arguments)
    {
        if (arguments.Length != 1)
        {
            Console.WriteLine("Usage: all <class>");
            return;
        }

        var className = arguments[0];
        if (!classDictionary.TryGetValue(className, out var classType))
        {
            Console.WriteLine($"Invalid class. Available classes: {string.Join(", ", classDictionary.Keys)}");
            return;
        }

        var method = typeof(DbContext).GetMethod(nameof(DbContext.Set), BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
        if (method == null)
        {
            Console.WriteLine("Unable to get the Set method from DbContext.");
            return;
        }

        var genericMethod = method.MakeGenericMethod(classType);
        var set = genericMethod.Invoke(context, null) as IEnumerable<object>;
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

    private static void Update(MyDbContext context, string[] arguments)
    {
        if (arguments.Length < 3)
        {
            Console.WriteLine("Usage: update <class> <id> <attribute1=value1> <attribute2=value2> ...");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = classDictionary[className];
        var instance = context.Find(classType, id);

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

        context.Update(instance);
        context.SaveChanges();

        Console.WriteLine($"{className} with ID {id} has been updated.");
    }
}
