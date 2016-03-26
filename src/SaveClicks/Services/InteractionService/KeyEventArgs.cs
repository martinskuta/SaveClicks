#region Usings

using System;
using SaveClicks.Services.CommandService;

#endregion

namespace SaveClicks.Services.InteractionService
{
    public class KeyShortcutArgs : EventArgs
    {
        public KeyShortcutArgs(KeyboardShortcut shortcut)
        {
            Shortcut = shortcut;
        }

        public KeyboardShortcut Shortcut { get; }
    }
}