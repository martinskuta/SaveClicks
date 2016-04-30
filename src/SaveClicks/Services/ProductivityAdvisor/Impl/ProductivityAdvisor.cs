#region Usings

using System;
using System.Linq;
using JetBrains.Annotations;
using SaveClicks.Services.CommandService;

#endregion

namespace SaveClicks.Services.ProductivityAdvisor.Impl
{
    [UsedImplicitly]
    public class ProductivityAdvisor : IProductivityAdvisor, IDisposable
    {
        private readonly ICommandService _commandService;
        private bool _disposed;

        public ProductivityAdvisor(ICommandService commandService)
        {
            _commandService = commandService;

            _commandService.CommandExecuted += CommandExecutedHandler;
            _commandService.CommandChainExecuted += CommandChainExecutedHandler;
        }

        public void Dispose()
        {
            if (_disposed) return;

            _commandService.CommandExecuted -= CommandExecutedHandler;
            _commandService.CommandChainExecuted -= CommandChainExecutedHandler;

            _disposed = true;
        }

        public event EventHandler<CommandArgs> UseKeyboardShortcut;

        private void CommandChainExecutedHandler(object sender, CommandChainArgs e)
        {
            if (e.Trigger != CommandTrigger.Mouse) return;

            var command = e.ChainedCommands.FirstOrDefault(cmd => !KnownChainTriggers.Contains(cmd.Name));
            if (command?.KeyboardBindings.Count == 0) return;

            RaiseUseKeyboardShortcut(command, e.Trigger);
        }

        private void CommandExecutedHandler(object sender, CommandArgs e)
        {
            if (e.Trigger == CommandTrigger.Mouse && e.Command.KeyboardBindings.Count > 0)
            {
                RaiseUseKeyboardShortcut(e.Command, e.Trigger);
            }
        }

        private void RaiseUseKeyboardShortcut(Command cmd, CommandTrigger trigger)
        {
            UseKeyboardShortcut?.Invoke(this, new CommandArgs(cmd, trigger));
        }
    }
}