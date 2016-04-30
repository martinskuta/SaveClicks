#region Usings

using System;
using System.Text;
using System.Windows.Input;

#endregion

namespace SaveClicks.Services.CommandService
{
    /// <summary>
    ///     Represents a combination of <see cref="ModifierKeys" /> and a single <see cref="Key" />
    /// </summary>
    public struct KeyboardShortcut : IEquatable<KeyboardShortcut>
    {
        private const string KeySeparator = "+";
        private string _displayString;

        public KeyboardShortcut(string key) : this(ModifierKeys.None, key)
        {
        }

        public KeyboardShortcut(ModifierKeys modifierKeys, string key)
        {
            Key = key;
            ModifierKeys = modifierKeys;
            _displayString = null;
        }

        public ModifierKeys ModifierKeys { get; }

        public string Key { get; }

        public bool Equals(KeyboardShortcut other)
        {
            return (int) ModifierKeys == (int) other.ModifierKeys && Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is KeyboardShortcut && Equals((KeyboardShortcut) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) ModifierKeys*397) ^ Key.GetHashCode();
            }
        }

        public override string ToString()
        {
            return _displayString ?? (_displayString = BuildDisplayString());
        }

        private string BuildDisplayString()
        {
            var sb = new StringBuilder();

            if (ModifierKeys.HasFlag(ModifierKeys.Control))
            {
                sb.Append("Ctrl");
                sb.Append(KeySeparator);
            }
            if (ModifierKeys.HasFlag(ModifierKeys.Shift))
            {
                sb.Append("Shift");
                sb.Append(KeySeparator);
            }
            if (ModifierKeys.HasFlag(ModifierKeys.Alt))
            {
                sb.Append("Alt");
                sb.Append(KeySeparator);
            }

            sb.Append(Key);

            return sb.ToString();
        }
    }
}