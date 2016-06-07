#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EnvDTE;
using JetBrains.Annotations;
using SaveClicks.Services.InteractionService;
using SaveClicks.Services.LogService;
using SaveClicks.System;

#endregion

namespace SaveClicks.Services.CommandService.Impl
{
    [UsedImplicitly]
    public sealed class VSCommandService : ICommandService, IDisposable
    {
        //Dependencies
        private DTE _dte;
        private readonly ILogService _log;
        private readonly ICommandBindingParser _bindingParser;
        private readonly IInteractionService _interactionService;

        //Command event buffer
        private IDisposable _bufferObservable;
        private readonly int _bufferSize = 500;
        private readonly TimeSpan _bufferTimeout = TimeSpan.FromMinutes(1);
        private readonly TimeSpan _slidingWindowSize = TimeSpan.FromSeconds(1);

        //User interaction
        private readonly RollingCache<KeyboardShortcut> _recentKeyShortcuts = new RollingCache<KeyboardShortcut>(2);
        private readonly TimeSpan _recentThreshold = TimeSpan.FromMilliseconds(100);
        private DateTime _lastMouseClick = DateTime.MinValue;

        private readonly object _chainReadLock = new object();
        private readonly List<CommandEventFootprint> _commandChainFootprints = new List<CommandEventFootprint>();
        // ReSharper disable once CollectionNeverQueried.Local
        private readonly List<CommandEvents> _commandEvents = new List<CommandEvents>();
        private bool _disposed;

        public VSCommandService(DTE dte, ICommandBindingParser bindingParser, IInteractionService interactionService, ILogService log)
        {
            _dte = dte;
            _bindingParser = bindingParser;
            _interactionService = interactionService;
            _log = log;

            SubscribeToCommandEvents();

            _interactionService.ShortcutPressed += OnShortcutPressed;
            _interactionService.LeftMouseButtonPressed += OnLeftMouseButtonPressed;
        }

        public event EventHandler<CommandArgs> CommandExecuted;
        public event EventHandler<CommandChainArgs> CommandChainExecuted;

        public void Dispose()
        {
            if (_disposed) return;

            _dte = null;
            _commandEvents.Clear();
            _bufferObservable.Dispose();

            _interactionService.ShortcutPressed -= OnShortcutPressed;
            _interactionService.LeftMouseButtonPressed -= OnLeftMouseButtonPressed;

            _disposed = true;
        }



        private void OnLeftMouseButtonPressed(object sender, EventArgs eventArgs)
        {
            _lastMouseClick = DateTime.UtcNow;
        }

        private void OnShortcutPressed(object sender, KeyShortcutArgs e)
        {
            _recentKeyShortcuts.Add(e.Shortcut);
        }

        private bool WasCommandShortcutPressed(IEnumerable<CommandBinding> commandBindings, KeyboardShortcut[] pressedShortcuts)
        {
            foreach (var commandBinding in commandBindings)
            {
                if (pressedShortcuts.Length < commandBinding.KeyCombinations.Count) continue;

                var idx = commandBinding.KeyCombinations.Count - 1;
                if (commandBinding.KeyCombinations.All(kc => kc.Equals(pressedShortcuts[idx--]))) return true;
            }
            return false;
        }

        private bool WasMouseClickedRecently()
        {
            return DateTime.UtcNow - _lastMouseClick < _recentThreshold;
        }

        private void SubscribeToCommandEvents()
        {
            var commandEvents = new Subject<CommandEventFootprint>();

            foreach (var command in _dte.Commands.Cast<EnvDTE.Command>())
            {
                if (string.IsNullOrWhiteSpace(command.Name)) continue;

                var commandEvent = _dte.Events.CommandEvents[command.Guid, command.ID];
                _commandEvents.Add(commandEvent);

                var beforeExecuteObservable = Observable.FromEvent<_dispCommandEvents_BeforeExecuteEventHandler, CommandEventFootprint>(
                    converter =>
                    {
                        _dispCommandEvents_BeforeExecuteEventHandler handler =
                            (string guid, int id, object cin, object cout, ref bool ddefault) =>
                            {
                                var wasMouseClickedRecently = WasMouseClickedRecently();
                                var shortcutsPressed = new KeyboardShortcut[_recentKeyShortcuts.Count];
                                _recentKeyShortcuts.CopyTo(shortcutsPressed, 0);

                                converter(new CommandEventFootprint(guid, id, CommandEventType.BeforeExecute, shortcutsPressed, wasMouseClickedRecently));
                            };
                        return handler;
                    },
                    handler => commandEvent.BeforeExecute += handler,
                    handler => commandEvent.BeforeExecute -= handler);

                var afterExecuteObservable = Observable.FromEvent<_dispCommandEvents_AfterExecuteEventHandler, CommandEventFootprint>(
                    converter =>
                    {
                        _dispCommandEvents_AfterExecuteEventHandler handler =
                            (guid, id, cin, cout) =>
                            {
                                var shortcutsPressed = new KeyboardShortcut[_recentKeyShortcuts.Count];
                                _recentKeyShortcuts.CopyTo(shortcutsPressed, 0);

                                converter(new CommandEventFootprint(guid, id, CommandEventType.AfterExecute, shortcutsPressed));
                            };
                        return handler;
                    },
                    handler => commandEvent.AfterExecute += handler,
                    handler => commandEvent.AfterExecute -= handler);

                beforeExecuteObservable.Merge(afterExecuteObservable).Subscribe(commandEvents);
            }

            _bufferObservable = commandEvents
                .Where(ft => !ExcludedCommands.Contains(ft.Guid, ft.Id))
                .BufferUntilCalm(_slidingWindowSize, _bufferSize, _bufferTimeout)
                .ObserveOn(ThreadPoolScheduler.Instance)
                .Subscribe(ProcessBuffer);
        }

        private Command CreateCommand(string guid, int id)
        {
            var command = _dte.Commands.Item(guid, id);

            var keyboardBindings = new List<CommandBinding>();

            var commandBindings = command.Bindings as object[];
            if (commandBindings != null)
            {
                keyboardBindings.AddRange(
                    commandBindings
                        .Select(cmdBinding => cmdBinding as string)
                        .Where(bindingString => !string.IsNullOrWhiteSpace(bindingString))
                        .Select(bindingString => _bindingParser.Parse(bindingString)));
            }

            return new Command(command.Name, keyboardBindings);
        }

        private void ProcessBuffer(IList<CommandEventFootprint> cmdFootprints)
        {
            if (cmdFootprints.Count == 0) return;

            List<CommandEventFootprint> chainSnapshot;
            lock (_chainReadLock)
            {
                _commandChainFootprints.AddRange(cmdFootprints);
                if (!IsCommandChainComplete()) return;

                chainSnapshot = _commandChainFootprints.ToList();
                _commandChainFootprints.Clear();
            }

            var chain = BuildCommandChain(chainSnapshot);
            if (chain == null)
            {
                if (_log.IsDebugEnabled) _log.LogDebug("Failed to build command chain.");
                return;
            }

            var trigger = GuessCommandsTrigger(chain[0], chainSnapshot[0]);
            _log.LogDebug($"Command chain triggered by {trigger}: {string.Join(", ", chain)}");

            if (chain.Count == 1)
            {
                CommandExecuted?.Invoke(this, new CommandArgs(chain[0], trigger));
            }
            else
            {
                CommandChainExecuted?.Invoke(this, new CommandChainArgs(chain, trigger));
            }
        }

        internal IList<Command> BuildCommandChain(List<CommandEventFootprint> chainFootprints)
        {
            var chain = TryBuildChainUsingBeforeAfterMatching(chainFootprints);
            if (chain != null) return chain;

            return FixChainUsingAfterBeforeMatchingWithTolerance(chainFootprints) ? TryBuildChainUsingBeforeAfterMatching(chainFootprints) : null;
        }

        /// <summary>
        ///   Builds the chain by matching before and after execute events of the commands, this is most precise and simple way
        ///   to get the order of the commands in the chain, but doesn't work always, because before execute events might be
        ///   missing if the command was canceled.
        /// </summary>
        /// <param name="chainFootprints">The chain footprints.</param>
        /// <returns>Returns the chain or null if it could not match all footprints.</returns>
        private IList<Command> TryBuildChainUsingBeforeAfterMatching(List<CommandEventFootprint> chainFootprints)
        {
            if (chainFootprints.Count % 2 != 0)
            {
                if (_log.IsDebugEnabled) _log.LogDebug("Command chain build failed: Odd number of footprints");
                return null;
            }

            var chain = new List<Command>();
            var cmdStack = new Stack<Command>();
            var ftStack = new Stack<string>();

            foreach (var footprint in chainFootprints)
            {
                if (footprint.Type == CommandEventType.BeforeExecute)
                {
                    ftStack.Push(footprint.UniqueIdentifier);
                    continue;
                }

                if (footprint.Type == CommandEventType.AfterExecute)
                {
                    //There is no matching before execute footprint for this after execute one
                    //the command was probably canceled by someone, so we cannot read the chain
                    if (ftStack.Count == 0 || ftStack.Pop() != footprint.UniqueIdentifier)
                    {
                        if (_log.IsDebugEnabled)
                            _log.LogDebug($"Command chain build failed: Missing matching BeforeExecute event for {footprint.UniqueIdentifier}");
                        return null;
                    }

                    cmdStack.Push(CreateCommand(footprint.Guid, footprint.Id));
                }

                if (ftStack.Count == 0)
                {
                    while (cmdStack.Count > 0)
                    {
                        chain.Add(cmdStack.Pop());
                    }
                }
            }

            if (ftStack.Count > 0)
            {
                if (_log.IsDebugEnabled)
                    _log.LogDebug($"Command chain build failed: Missing matching BeforeExecute event for {string.Join(", ", ftStack)}");
                return null;
            }

            return chain;
        }

        private bool FixChainUsingAfterBeforeMatchingWithTolerance(List<CommandEventFootprint> chainFootprints)
        {
            var ftStack = new Stack<CommandEventFootprint>();

            for (var i = chainFootprints.Count - 1; i >= 0; i--)
            {
                var footprint = chainFootprints[i];

                if (footprint.Type == CommandEventType.AfterExecute)
                {
                    ftStack.Push(footprint);
                    continue;
                }

                if (footprint.Type == CommandEventType.BeforeExecute)
                {
                    if (ftStack.Count == 0)
                    {
                        if (_log.IsDebugEnabled)
                            _log.LogDebug(
                                $"Fix of command event footprint chain failed: Could not determine order of BeforeExecute of {footprint.UniqueIdentifier}");
                        return false;
                    }

                    var current = ftStack.Pop();
                    if (current.UniqueIdentifier == footprint.UniqueIdentifier) continue;

                    if (ftStack.Count == 0 || ftStack.Peek().UniqueIdentifier == footprint.UniqueIdentifier)
                    {
                        chainFootprints.Insert(++i, new CommandEventFootprint(current.Guid, current.Id, CommandEventType.BeforeExecute, current.ShortcutPressed));
                    }
                    else
                    {
                        if (_log.IsDebugEnabled)
                            _log.LogDebug(
                                $"Fix of command event footprint chain failed: Could not determine order of BeforeExecute of {footprint.UniqueIdentifier}");
                        return false;
                    }
                }
            }

            while (ftStack.Count > 0)
            {
                var current = ftStack.Pop();
                chainFootprints.Insert(0, new CommandEventFootprint(current.Guid, current.Id, CommandEventType.BeforeExecute, current.ShortcutPressed));
            }

            return true;
        }

        /// <summary>
        ///   We just basically check here if there is no command that fired before execute, but doesn't have after execute
        ///   counterpart.
        /// </summary>
        /// <remarks>
        ///   Note that command will have only after execute if it was canceled or replaced (looking at you R#)
        /// </remarks>
        /// <returns></returns>
        private bool IsCommandChainComplete()
        {
            var tracker = new HashSet<Tuple<string, int>>();

            foreach (var footprint in _commandChainFootprints)
            {
                var cmdId = new Tuple<string, int>(footprint.Guid, footprint.Id);
                if (footprint.Type == CommandEventType.BeforeExecute)
                {
                    tracker.Add(cmdId);
                }
                else
                {
                    tracker.Remove(cmdId);
                }
            }

            if (_log.IsDebugEnabled && tracker.Count > 0)
            {
                _log.LogDebug($"Command chain incomplete, waiting for {string.Join(", ", tracker.Select(ft => CreateCommand(ft.Item1, ft.Item2)))}.");
            }
            return tracker.Count == 0;
        }

        private CommandTrigger GuessCommandsTrigger(Command cmd, CommandEventFootprint footprint)
        {
            if (cmd.KeyboardBindings.Count > 0 && WasCommandShortcutPressed(cmd.KeyboardBindings, footprint.ShortcutPressed))
            {
                return CommandTrigger.Keyboard;
            }
            return footprint.WasMouseClickedRecently.HasValue && footprint.WasMouseClickedRecently.Value ? CommandTrigger.Mouse : CommandTrigger.Unknown;
        }

        internal class CommandEventFootprint
        {
            private string _uniqueIdentifier;

            public CommandEventFootprint(string guid, int id, CommandEventType type, KeyboardShortcut[] shortcutPressed, bool? wasMouseClickedRecently = null)
            {
                Guid = guid;
                Id = id;
                Type = type;
                ShortcutPressed = shortcutPressed;
                WasMouseClickedRecently = wasMouseClickedRecently;
            }

            public string Guid { get; }

            public int Id { get; }

            public string UniqueIdentifier => _uniqueIdentifier ?? (_uniqueIdentifier = Guid + Id);

            public CommandEventType Type { get; }

            public bool? WasMouseClickedRecently { get; }

            public KeyboardShortcut[] ShortcutPressed { get; }
        }

        internal enum CommandEventType : byte
        {
            BeforeExecute,
            AfterExecute
        }
    }
}