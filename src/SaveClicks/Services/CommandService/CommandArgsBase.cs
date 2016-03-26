using System;

namespace SaveClicks.Services.CommandService
{
    public abstract class CommandArgsBase : EventArgs
    {
        protected CommandArgsBase(CommandTrigger trigger)
        {
            Trigger = trigger;
        }

        public CommandTrigger Trigger { get; }
    }
}