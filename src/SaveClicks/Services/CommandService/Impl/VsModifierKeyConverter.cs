#region Usings

using System;
using System.Windows.Input;
using JetBrains.Annotations;

#endregion

namespace SaveClicks.Services.CommandService.Impl
{
    [UsedImplicitly]
    public class VsModifierKeyConverter : IModifierKeyConverter
    {
        public string ConvertToString(ModifierKeys key)
        {
            switch (key)
            {
                case ModifierKeys.Alt:
                    return "Alt";
                case ModifierKeys.Control:
                    return "Ctrl";
                case ModifierKeys.Shift:
                    return "Shift";
            }

            return null;
        }

        public ModifierKeys ConvertToKey(string key)
        {
            if (key == "Ctrl") return ModifierKeys.Control;

            return (ModifierKeys) Enum.Parse(typeof(ModifierKeys), key);
        }
    }
}