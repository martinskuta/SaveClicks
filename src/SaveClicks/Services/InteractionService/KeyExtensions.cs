#region Usings

using System;
using System.Text;
using System.Windows.Input;
using SaveClicks.System;

#endregion

namespace SaveClicks.Services.InteractionService
{
    public static class KeyExtensions
    {
        public static bool IsModifier(this Key key)
        {
            switch (key)
            {
                case Key.LWin:
                case Key.RWin:
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.LeftAlt:
                case Key.RightAlt:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        ///     Gets the text that user would see in notepad when he presses that key.
        /// </summary>
        /// <param name="key">The key.</param>
        public static string GetText(this Key key)
        {
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var keyboardState = new byte[256];

            const byte keyPressed = 0x80;
            keyboardState[virtualKey] = keyPressed;

            var scanCode = NativeMethods.MapVirtualKey((uint) virtualKey);
            var stringBuilder = new StringBuilder(32);

            var result = NativeMethods.ToUnicode((uint) virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity);
            return result < 1 ? null : stringBuilder.ToString();
        }
    }
}