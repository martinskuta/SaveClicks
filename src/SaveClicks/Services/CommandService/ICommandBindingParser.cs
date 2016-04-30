namespace SaveClicks.Services.CommandService
{
    public interface ICommandBindingParser
    {
        CommandBinding Parse(string binding);
    }
}