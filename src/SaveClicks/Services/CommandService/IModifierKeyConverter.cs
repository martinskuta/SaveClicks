#region Usings

using System.Windows.Input;

#endregion

namespace SaveClicks.Services.CommandService
{
    public interface IModifierKeyConverter
    {
        string ConvertToString(ModifierKeys key);

        ModifierKeys ConvertToKey(string key);
    }
}