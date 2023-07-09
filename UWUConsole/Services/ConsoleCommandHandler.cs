// ConsoleCommandHandler.cs

public class ConsoleCommandHandler
{
    private readonly IDatabaseOperations _databaseOperations;

    public ConsoleCommandHandler(IDatabaseOperations databaseOperations)
    {
        _databaseOperations = databaseOperations;
    }

    public async Task HandleCommandAsync(string command, string[] arguments)
    {
        switch (command)
        {
            case "create":
                await _databaseOperations.CreateAsync(arguments);
                break;
            case "show":
                await _databaseOperations.ShowAsync(arguments);
                break;
            case "destroy":
                await _databaseOperations.DestroyAsync(arguments);
                break;
            case "all":
                await _databaseOperations.AllAsync(arguments);
                break;
            case "update":
                await _databaseOperations.UpdateAsync(arguments);
                break;
            case "quit":
                Environment.Exit(0);
                break;
            default:
                Console.WriteLine($"Unknown command: {command}");
                break;
        }
    }
}
