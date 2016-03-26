#region Usings

using System.Collections.Generic;

#endregion

namespace SaveClicks.Services.CommandService
{
    public class Command
    {
        public Command(string name, IList<CommandBinding> keyboardBindings)
        {
            Name = name;
            KeyboardBindings = keyboardBindings;
        }

        public string Name { get; }

        public IList<CommandBinding> KeyboardBindings { get; }

        private bool Equals(Command other)
        {
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Command) obj);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            if (KeyboardBindings.Count == 0)
            {
                return $"{Name} (No shortcut)";
            }

            return $"{Name} ({KeyboardBindings[0].ToString("ns")})";
        }
    }
}