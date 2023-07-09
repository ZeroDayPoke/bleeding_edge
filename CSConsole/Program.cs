// Program.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

public class Program
{
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

        var classDictionary = new Dictionary<string, Type>
        {
            { "User", typeof(User) },
            { "Role", typeof(Role) }
        };

        services.AddSingleton(classDictionary);
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IDatabaseOperations, DatabaseOperations>();
        services.AddTransient<ConsoleCommandHandler>();

        var provider = services.BuildServiceProvider();

        using var context = provider.GetRequiredService<MyDbContext>();
        var commandHandler = provider.GetRequiredService<ConsoleCommandHandler>();

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
            if (input == null)
            {
                Console.WriteLine("Input is null. Please provide valid input.");
                return;
            }

            var command = input.Split(' ')[0];
            var arguments = input.Substring(command.Length).Trim().Split(' ');

            commandHandler.HandleCommand(command, arguments);
        }
    }
}
