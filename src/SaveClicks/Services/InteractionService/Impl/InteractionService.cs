#region Usings

using System;
using System.Windows.Forms;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using JetBrains.Annotations;
using SaveClicks.Services.CommandService;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

#endregion

namespace SaveClicks.Services.InteractionService.Impl
{
    [UsedImplicitly]
    public class InteractionService : IInteractionService, IDisposable
    {
        private readonly IKeyboardMouseEvents _keyboardMouseEvents;
        private readonly IKeyConverter _keyConverter;
        private bool _disposed;

        public InteractionService(IKeyConverter keyConverter)
        {
            _keyConverter = keyConverter;
            _keyboardMouseEvents = Hook.AppEvents();

            _keyboardMouseEvents.KeyDown += OnKeyDown;
            _keyboardMouseEvents.MouseClick += OnMouseClick;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _keyboardMouseEvents.MouseClick -= OnMouseClick;
            _keyboardMouseEvents.KeyDown -= OnKeyDown;
            _keyboardMouseEvents.Dispose();

            _disposed = true;
        }

        public event EventHandler<KeyShortcutArgs> ShortcutPressed;

        public event EventHandler<EventArgs> LeftMouseButtonPressed;

        private void OnMouseClick(object sender, MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.Button == MouseButtons.Left)
                LeftMouseButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        private void OnKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (ShortcutPressed == null) return;

            var key = KeyInterop.KeyFromVirtualKey(keyEventArgs.KeyValue);
            if (key.IsModifier()) return;

            var modifiers = ModifierKeys.None;
            if (keyEventArgs.Alt) modifiers |= ModifierKeys.Alt;
            if (keyEventArgs.Control) modifiers |= ModifierKeys.Control;
            if (keyEventArgs.Shift) modifiers |= ModifierKeys.Shift;

            var shortcut = new KeyboardShortcut(modifiers, _keyConverter.ConvertToString(key));

            ShortcutPressed?.Invoke(this, new KeyShortcutArgs(shortcut));
        }
    }
}