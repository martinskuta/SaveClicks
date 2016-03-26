using System;
using SaveClicks.Services.CommandService;

namespace SaveClicks.Services.ProductivityAdvisor
{
    public interface IProductivityAdvisor
    {
        event EventHandler<CommandArgs> UseKeyboardShortcut;
    }
}