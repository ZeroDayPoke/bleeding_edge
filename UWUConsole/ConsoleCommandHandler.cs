// ConsoleCommandHandler.cs

public class ConsoleCommandHandler
{
    private readonly IDatabaseOperations _databaseOperations;

    public ConsoleCommandHandler(IDatabaseOperations databaseOperations)
    {
        _databaseOperations = databaseOperations;
    }

    public void HandleCommand(string command, string[] arguments)
    {
        switch (command)
        {
            case "create":
                _databaseOperations.Create(arguments);
                break;
            case "show":
                _databaseOperations.Show(arguments);
                break;
            case "destroy":
                _databaseOperations.Destroy(arguments);
                break;
            case "all":
                _databaseOperations.All(arguments);
                break;
            case "update":
                _databaseOperations.Update(arguments);
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
