#region Usings

using System;
using EnvDTE;
using JetBrains.Annotations;
using Microsoft.VisualStudio.Shell;

#endregion

namespace SaveClicks.Services.LogService.Impl
{
    [UsedImplicitly]
    public class VsOutputWindow : ILogService, IDisposable
    {
        private const string OutputWindowName = "Save clicks";

        private DTE _dte;
        private OutputWindowPane _outputWindowPane;
        private bool _disposed;

        public VsOutputWindow(DTE dte)
        {
            _dte = dte;
        }

        private OutputWindowPane OutputWindow => _outputWindowPane ?? (_outputWindowPane = GetOutputWindowPane());

        public void Dispose()
        {
            if (_disposed) return;

            _dte = null;
            _outputWindowPane = null;

            _disposed = true;
        }

        public bool IsErrorEnabled => true;

        public void LogError(string errorMsg)
        {
            OutputString(errorMsg);
        }

        public void LogException(Exception ex)
        {
            OutputString(ex.ToString());
        }

        public void LogException(string errorMsg, Exception ex)
        {
            OutputString($"{errorMsg}{Environment.NewLine}{Environment.NewLine}{ex}");
        }

        public bool IsInfoEnabled => true;

        public void LogInfo(string infoMsg)
        {
            OutputString(infoMsg);
        }

        public bool IsDebugEnabled => true;

        public void LogDebug(string debugMsg)
        {
            OutputString(debugMsg);
        }

        private void OutputString(string message)
        {
            try
            {
                OutputWindow.OutputString(message + Environment.NewLine);
            }
            catch (Exception ex)
            {
                ActivityLog.LogError(ToString(), $"Failed to log message: {message}{Environment.NewLine}{Environment.NewLine}{ex}");
            }
        }

        private OutputWindowPane GetOutputWindowPane()
        {
            if (_outputWindowPane == null)
            {
                var outputWindow = (OutputWindow)_dte.Windows.Item(Constants.vsWindowKindOutput).Object;
                _outputWindowPane = outputWindow.OutputWindowPanes.Add(OutputWindowName);
            }

            return _outputWindowPane;
        }
    }
}