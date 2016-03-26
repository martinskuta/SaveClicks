#region Usings

using System;

#endregion

namespace SaveClicks.Services.CommandService
{
    public class CommandArgs : CommandArgsBase
    {
        public CommandArgs(Command cmd, CommandTrigger trigger) : base(trigger)
        {
            if (cmd == null) throw new ArgumentNullException(nameof(cmd));

            Command = cmd;
        }

        public Command Command { get; }
    }
}