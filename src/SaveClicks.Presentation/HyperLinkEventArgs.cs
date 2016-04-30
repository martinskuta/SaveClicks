#region Usings

using System;

#endregion

namespace SaveClicks.Presentation
{
    public class HyperLinkEventArgs : EventArgs
    {
        public HyperLinkEventArgs(object hyperlinkObjectForRaisedEvent)
            : this()
        {
            HyperlinkObjectForRaisedEvent = hyperlinkObjectForRaisedEvent;
        }

        public HyperLinkEventArgs()
        {
        }

        /// <summary>
        ///     Gets or sets the hyper link object for raised event.  This object will be passed back when
        ///     the control raises the HyperlinkClicked event.
        /// </summary>
        /// <value>
        ///     The hyper link object for raised event.
        /// </value>
        public object HyperlinkObjectForRaisedEvent { get; set; }
    }
}