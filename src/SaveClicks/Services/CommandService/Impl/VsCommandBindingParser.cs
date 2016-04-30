#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;

#endregion

namespace SaveClicks.Services.CommandService.Impl
{
    /// <summary>
    ///   This class parses the command binding strings from Visual Studio API to <see cref="CommandBinding" /> class.
    /// </summary>
    /// <seealso cref="ICommandBindingParser" />
    [UsedImplicitly]
    public class VsCommandBindingParser : ICommandBindingParser
    {
        private const string KeySeparator = "+";
        private const string ChordSeparator = ", ";
        private const string ScopeSeparator = "::";

        private readonly IModifierKeyConverter _modifierKeyConverter;

        public VsCommandBindingParser(IModifierKeyConverter modifierKeyConverter)
        {
            _modifierKeyConverter = modifierKeyConverter;
        }

        public CommandBinding Parse(string binding)
        {
            if (string.IsNullOrWhiteSpace(binding))
                throw new ArgumentException(@"Binding string cannot be null, empty or whitespace.", nameof(binding));

            var indexOfScopeSeparator = binding.IndexOf(ScopeSeparator, StringComparison.Ordinal);

            if (indexOfScopeSeparator < 0)
                throw new FormatException($"The binding string '{binding}' is not in correct format. Scope separator '{ScopeSeparator}' was not found.");

            var scope = binding.Substring(0, indexOfScopeSeparator);
            var bindingWithoutScope = binding.Substring(indexOfScopeSeparator + ScopeSeparator.Length);

            var keyCombinationsStr = bindingWithoutScope.Split(new[] { ChordSeparator }, StringSplitOptions.RemoveEmptyEntries);

            return new CommandBinding(scope, keyCombinationsStr.Select(ParseKeyboardShortcut));
        }

        private KeyboardShortcut ParseKeyboardShortcut(string keyCombinationStr)
        {
            var indexOfKeySeparator = keyCombinationStr.IndexOf(KeySeparator, StringComparison.Ordinal);
            if (indexOfKeySeparator == -1) return new KeyboardShortcut(keyCombinationStr);

            var keyParsingBuffer = new Stack<string>();
            var index = 0;
            do
            {
                keyParsingBuffer.Push(keyCombinationStr.Substring(index, indexOfKeySeparator - index));

                index = indexOfKeySeparator + KeySeparator.Length;
                indexOfKeySeparator = keyCombinationStr.IndexOf(KeySeparator, index, StringComparison.Ordinal);
            }
            while (indexOfKeySeparator != -1);
            keyParsingBuffer.Push(keyCombinationStr.Substring(index));

            var key = keyParsingBuffer.Pop();

            var modifierKeys = ModifierKeys.None;
            while (keyParsingBuffer.Count > 0)
            {
                modifierKeys |= _modifierKeyConverter.ConvertToKey(keyParsingBuffer.Pop());
            }

            return new KeyboardShortcut(modifierKeys, key);
        }
    }
}