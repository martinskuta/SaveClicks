#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace SaveClicks.Presentation
{
    public partial class ToastPopup
    {
        #region Public Methods

        /// <summary>
        ///     Opens a window and returns without waiting for the newly opened window to close.
        /// </summary>
        public new void Show()
        {
            var focusedElement = Keyboard.FocusedElement;

            Topmost = true;
            base.Show();

            Owner = Application.Current.MainWindow;
            Owner.SizeChanged += (s, e) => AdjustWindows();
            Owner.StateChanged += (s, e) => AdjustWindows();
            Owner.LocationChanged += (s, e) => AdjustWindows();
            Closed += NotificationWindowClosed;
            AdjustWindows();

            if (focusedElement != null)
            {
                // Restore keyboard focus to the original element that had focus. That way if someone
                // was typing into a control we don't steal keyboard focus away from that control.
                focusedElement.Focusable = true;
                Keyboard.Focus(focusedElement);
            }
        }

        #endregion

        #region Members

        private readonly string _name = typeof(ToastPopup).Name;
        private volatile object _lockObject = new object();
        private string _title;

        #endregion Members

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPopup" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        public ToastPopup(string title, string text)
            : this(title)
        {
            TextBoxShortDescription.Text = text;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPopup" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="textInlines"></param>
        public ToastPopup(string title, List<Inline> textInlines)
            : this(title)
        {
            TextBoxShortDescription.Inlines.AddRange(textInlines);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPopup" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        /// <param name="hyperlinkText">The hyperlink text.</param>
        /// <param name="hyperlinkObjectForRaisedEvent">The hyperlink object for raised event.</param>
        public ToastPopup(string title, string text, string hyperlinkText, object hyperlinkObjectForRaisedEvent = null)
            : this(title, text)
        {
            HyperlinkObjectForRaisedEvent = hyperlinkObjectForRaisedEvent;
            SetHyperLinkButton(hyperlinkText);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPopup" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="textInlines">The text inlines.</param>
        /// <param name="hyperlinkText">The hyperlink text.</param>
        /// <param name="hyperlinkObjectForRaisedEvent">The hyperlink object for raised event.</param>
        public ToastPopup(string title, List<Inline> textInlines, string hyperlinkText, object hyperlinkObjectForRaisedEvent = null)
            : this(title)
        {
            HyperlinkObjectForRaisedEvent = hyperlinkObjectForRaisedEvent;
            TextBoxShortDescription.Inlines.AddRange(textInlines);
            SetHyperLinkButton(hyperlinkText);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ToastPopup" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        private ToastPopup(string title)
        {
            InitializeComponent();
            Application.Current.MainWindow.Closing += MainWindowClosing;

            TextBoxTitle.Text = title;
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        ///     Gets or sets the color of the font.
        /// </summary>
        /// <value>
        ///     The color of the font.
        /// </value>
        public Brush FontColor
        {
            get { return TextBoxTitle.Foreground; }

            set
            {
                TextBoxTitle.Foreground = value;
                TextBoxShortDescription.Foreground = value;
            }
        }

        /// <summary>
        ///     Gets or sets a brush that describes the border background of a control.
        /// </summary>
        public new Brush BorderBrush
        {
            get { return BorderBackground.BorderBrush; }

            set { BorderBackground.BorderBrush = value; }
        }

        /// <summary>
        ///     Gets or sets a brush that describes the background of a control.
        /// </summary>
        public new Brush Background
        {
            get { return BorderBackground.Background; }

            set { BorderBackground.Background = value; }
        }

        /// <summary>
        ///     Gets or sets the hyperlink object for raised event.  This object will be passed back when
        ///     the control raises the HyperlinkClicked event.
        /// </summary>
        /// <value>
        ///     The hyperlink object for raised event.
        /// </value>
        public object HyperlinkObjectForRaisedEvent { get; set; }

        #endregion Public Properties

        #region Events

        public event EventHandler<EventArgs> ClosedByUser;

        public event EventHandler<HyperLinkEventArgs> HyperlinkClicked;

        #endregion Events

        #region Event Handlers

        /// <summary>
        ///     Raises the <see cref="E:ClosedByUser" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnClosedByUser(EventArgs e)
        {
            ClosedByUser?.Invoke(this, e);
        }

        /// <summary>
        ///     Raises the <see cref="E:HyperlinkClicked" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected virtual void OnHyperlinkClicked(HyperLinkEventArgs e)
        {
            HyperlinkClicked?.Invoke(this, e);
        }

        /// <summary>
        ///     Buttons the view click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        private void ButtonViewClick(object sender, RoutedEventArgs e)
        {
            OnHyperlinkClicked(new HyperLinkEventArgs {HyperlinkObjectForRaisedEvent = HyperlinkObjectForRaisedEvent});
        }

        /// <summary>
        ///     Doubles the animation completed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void DoubleAnimationCompleted(object sender, EventArgs e)
        {
            if (!IsMouseOver)
            {
                Close();
            }
        }

        /// <summary>
        ///     Images the mouse up.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs" /> instance containing the event data.</param>
        private void ImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            OnClosedByUser(new EventArgs());
            Close();
        }

        /// <summary>
        ///     Mains the window closing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs" /> instance containing the event data.</param>
        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var windowType = window.GetType().Name;
                if (windowType.Equals(_name))
                {
                    window.Close();
                }
            }
        }

        private void NotificationWindowClosed(object sender, EventArgs e)
        {
            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (windowName.Equals(_name) && !ReferenceEquals(window, this))
                {
                    // Adjust any windows that were above this one to drop down
                    if (window.Top < Top && window.Left == Left)
                    {
                        window.Top = window.Top + ActualHeight;

                        if (!WindowsExistToTheRight(Left))
                        {
                            window.Left = window.Left + ActualWidth;
                        }
                    }
                }
            }

            AdjustWindows();
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        ///     Dow windows exist to the right.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <returns></returns>
        private bool WindowsExistToTheRight(double left)
        {
            var mainWindowRect = WindowHelpers.GetMainWindowRect();

            foreach (Window window in Application.Current.Windows)
            {
                var windowName = window.GetType().Name;

                if (windowName.Equals(_name) &&
                    !ReferenceEquals(window, this) &&
                    Math.Abs(left - (mainWindowRect.Right - Width)) < 1)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Adjusts the windows.
        /// </summary>
        private void AdjustWindows()
        {
            lock (_lockObject)
            {
                var mainWindowRect = WindowHelpers.GetMainWindowRect();

                Left = mainWindowRect.Right - ActualWidth;
                var top = mainWindowRect.Bottom - ActualHeight;

                foreach (Window window in Application.Current.Windows)
                {
                    var windowName = window.GetType().Name;

                    if (windowName.Equals(_name) && !ReferenceEquals(window, this))
                    {
                        window.Topmost = true;

                        if (Math.Abs(Left - window.Left) < 1)
                        {
                            top = top - window.ActualHeight;
                        }

                        if (top < mainWindowRect.Top)
                        {
                            Left = Left - ActualWidth;
                            top = mainWindowRect.Bottom - ActualHeight;
                        }
                    }
                }

                Top = top;
            }
        }

        /// <summary>
        ///     Sets the hyperlink button.
        /// </summary>
        /// <param name="hyperlinkText">The hyperlink text.</param>
        private void SetHyperLinkButton(string hyperlinkText)
        {
            if (!string.IsNullOrWhiteSpace(hyperlinkText))
            {
                ButtonView.Content = hyperlinkText;
                ButtonView.Visibility = Visibility.Visible;
            }
        }

        #endregion Private Methods
    }
}