// Services/DatabaseOperations.cs

using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Threading.Tasks;

public interface IDatabaseOperations
{
    Task CreateAsync(string[] arguments);
    Task ShowAsync(string[] arguments);
    Task DestroyAsync(string[] arguments);
    Task AllAsync(string[] arguments);
    Task UpdateAsync(string[] arguments);
    Task RegisterUserAsync(string username, string password);
    Task<User> LoginUserAsync(string username, string password);
    Task<User> VerifyUserAsync(string token);
}

public class DatabaseOperations : IDatabaseOperations
{
    private readonly MyDbContext _context;
    private readonly IEmailService _emailService;
    private readonly Dictionary<string, Type> _classDictionary;

    public DatabaseOperations(MyDbContext context, IEmailService emailService, Dictionary<string, Type> classDictionary)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _classDictionary = classDictionary ?? throw new ArgumentNullException(nameof(classDictionary));
    }

    public async Task CreateAsync(string[] arguments)
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
                await _context.SaveChangesAsync();

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

    public async Task ShowAsync(string[] arguments)
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
        var instance = await _context.FindAsync(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        Console.WriteLine(instance);
    }

    public async Task DestroyAsync(string[] arguments)
    {
        if (arguments.Length != 2)
        {
            Console.WriteLine("Usage: destroy <class> <id>");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = _classDictionary[className];
        var instance = await _context.FindAsync(classType, id);

        if (instance == null)
        {
            Console.WriteLine($"{className} with ID {id} not found");
            return;
        }

        _context.Remove(instance);
        await _context.SaveChangesAsync();

        Console.WriteLine($"{className} with ID {id} has been deleted.");
    }

    public async Task UpdateAsync(string[] arguments)
    {
        if (arguments.Length < 3)
        {
            Console.WriteLine("Usage: update <class> <id> <attribute1=value1> <attribute2=value2> ...");
            return;
        }

        var className = arguments[0];
        var id = Convert.ToInt32(arguments[1]);

        var classType = _classDictionary[className];
        var instance = await _context.FindAsync(classType, id);

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
        await _context.SaveChangesAsync();

        Console.WriteLine($"{className} with ID {id} has been updated.");
    }

    public async Task AllAsync(string[] arguments)
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


    public async Task RegisterUserAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Username and password cannot be null or empty.");
        }

        var user = new User { Username = username };
        user.SetPassword(password);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        _emailService?.SendVerificationEmail(user.Email, user.VerificationToken);
    }

    public async Task<User> LoginUserAsync(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new ArgumentException("Username and password cannot be null or empty.");
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username);
        if (user != null && user.VerifyPassword(password))
        {
            return user;
        }
        else
        {
            throw new Exception("Invalid username or password");
        }
    }

    public async Task<User> VerifyUserAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.");
        }

        var user = await _context.Users.SingleOrDefaultAsync(u => u.VerificationToken == token);
        if (user != null)
        {
            user.IsEmailVerified = true;
            await _context.SaveChangesAsync();
            return user;
        }
        else
        {
            throw new Exception("Invalid verification token");
        }
    }
}
