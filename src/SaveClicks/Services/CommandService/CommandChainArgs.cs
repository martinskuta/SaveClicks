#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace SaveClicks.Services.CommandService
{
    public class CommandChainArgs : CommandArgsBase
    {
        public CommandChainArgs(IList<Command> chain, CommandTrigger trigger) : base(trigger)
        {
            if (chain == null) throw new ArgumentNullException(nameof(chain));
            if (chain.Count < 2) throw new ArgumentException("There must be at least two commands to form a command chain.");

            ChainedCommands = chain;
        }

        public IList<Command> ChainedCommands { get; }
    }
}