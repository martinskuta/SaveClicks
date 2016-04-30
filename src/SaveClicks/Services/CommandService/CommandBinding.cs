#region Usings

using System.Collections.Generic;
using System.Linq;

#endregion

namespace SaveClicks.Services.CommandService
{
    public class CommandBinding
    {
        private const string KeyCombSeparator = ", ";
        private const string ScopeSeparator = "::";

        public CommandBinding(string scope, IEnumerable<KeyboardShortcut> keyCombinations)
        {
            Scope = scope;
            KeyCombinations = keyCombinations.ToList();
        }

        public string Scope { get; }

        public IList<KeyboardShortcut> KeyCombinations { get; }

        public override string ToString()
        {
            return $"{Scope}{ScopeSeparator}{string.Join(KeyCombSeparator, KeyCombinations)}";
        }

        public string ToString(string formatter)
        {
            if (formatter == "ns") return string.Join(KeyCombSeparator, KeyCombinations);

            return ToString();
        }
    }
}