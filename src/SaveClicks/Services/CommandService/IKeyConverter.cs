#region Usings

using System.Windows.Input;

#endregion

namespace SaveClicks.Services.CommandService
{
    public interface IKeyConverter
    {
        string ConvertToString(Key key);
    }
}