#region Usings

using System;

#endregion

namespace SaveClicks.Services.CommandService
{
    public interface ICommandService
    {
        event EventHandler<CommandArgs> CommandExecuted;

        event EventHandler<CommandChainArgs> CommandChainExecuted;
    }
}