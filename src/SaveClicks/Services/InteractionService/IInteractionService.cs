#region Usings

using System;

#endregion

namespace SaveClicks.Services.InteractionService
{
    public interface IInteractionService
    {
        event EventHandler<KeyShortcutArgs> ShortcutPressed;

        event EventHandler<EventArgs> LeftMouseButtonPressed;
    }
}