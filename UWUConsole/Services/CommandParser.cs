// CommandParser.cs

public interface ICommandParser
{
    (string, string[]) Parse(string input);
}

public class CommandParser : ICommandParser
{
    public (string, string[]) Parse(string input)
    {
        var parts = input.Split(' ');
        var command = parts[0];
        var arguments = parts.Skip(1).ToArray();
        return (command, arguments);
    }
}
