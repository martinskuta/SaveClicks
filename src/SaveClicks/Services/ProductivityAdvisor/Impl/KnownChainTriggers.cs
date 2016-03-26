#region Usings

using System.Collections.Generic;

#endregion

namespace SaveClicks.Services.ProductivityAdvisor.Impl
{
    /// <summary>
    ///     Contains list of commands that are dummy click commands that just delegate to other commands.
    ///     For example when you click on the play button to debug an application, the command that gets executed is
    ///     Debug.StartDebugTarget, which basically just calls what is selected there eg Debug.Start, which is the command we
    ///     are interested in not the actual Debug.StartDebugTarget.
    /// </summary>
    public static class KnownChainTriggers
    {
        private static readonly ISet<string> KnownTriggers = new HashSet<string>
        {
            "Debug.StartDebugTarget"
        };

        public static bool Contains(string cmdName)
        {
            return KnownTriggers.Contains(cmdName);
        }
    }
}