#region using

using System.Collections.Generic;

#endregion

namespace SaveClicks.Services.CommandService.Impl
{
    /// <summary>
    ///   Contains commands that are globally excluded in the extension from being monitored.
    /// </summary>
    internal static class ExcludedCommands
    {
        private static readonly ISet<string> IgnoredGuids = new HashSet<string>
        {
            "{C9DD4A59-47FB-11D2-83E7-00C04F9902C1}278", //debug.locationtoolbar.processcombo
            "{5EFC7975-14BC-11CF-9B2B-00AA00573819}684", //build.solutionconfigurations
            "{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}1990" //build.solutionplatforms
        };

        public static bool Contains(string guid, int id)
        {
            return IgnoredGuids.Contains(guid + id);
        }
    }
}