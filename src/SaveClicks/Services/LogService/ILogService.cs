using System;

namespace SaveClicks.Services.LogService
{
    public interface ILogService
    {
        bool IsErrorEnabled { get; }

        bool IsInfoEnabled { get; }

        bool IsDebugEnabled { get; }

        void LogError(string errorMsg);

        void LogException(Exception ex);

        void LogException(string errorMsg, Exception ex);

        void LogInfo(string infoMsg);

        void LogDebug(string debugMsg);
    }
}