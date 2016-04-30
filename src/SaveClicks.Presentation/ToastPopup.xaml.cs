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
        private readonly string _name = typeof(ToastPopup).Name;
        private volatile object _lockObject = new object();

        public ToastPopup(string title, string text)
            : this(title)
        {
            TextBoxShortDescription.Text = text;
        }

        public ToastPopup(string title, List<Inline> textInlines)
            : this(title)
        {
            TextBoxShortDescription.Inlines.AddRange(textInlines);
        }

        public ToastPopup(string title, string text, string hyperlinkText, object hyperlinkObjectForRaisedEvent = null)
            : this(title, text)
        {
            HyperlinkObjectForRaisedEvent = hyperlinkObjectForRaisedEvent;
            SetHyperLinkButton(hyperlinkText);
        }

        public ToastPopup(string title, List<Inline> textInlines, string hyperlinkText, object hyperlinkObjectForRaisedEvent = null)
            : this(title)
        {
            HyperlinkObjectForRaisedEvent = hyperlinkObjectForRaisedEvent;
            TextBoxShortDescription.Inlines.AddRange(textInlines);
            SetHyperLinkButton(hyperlinkText);
        }

        private ToastPopup(string title)
        {
            InitializeComponent();
            Application.Current.MainWindow.Closing += MainWindowClosing;

            TextBoxTitle.Text = title;
        }

        public Brush FontColor
        {
            get { return TextBoxTitle.Foreground; }

            set
            {
                TextBoxTitle.Foreground = value;
                TextBoxShortDescription.Foreground = value;
            }
        }

        public new Brush BorderBrush
        {
            get { return BorderBackground.BorderBrush; }

            set { BorderBackground.BorderBrush = value; }
        }

        public new Brush Background
        {
            get { return BorderBackground.Background; }

            set { BorderBackground.Background = value; }
        }

        public object HyperlinkObjectForRaisedEvent { get; set; }

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

        public event EventHandler<EventArgs> ClosedByUser;

        public event EventHandler<HyperLinkEventArgs> HyperlinkClicked;

        private void OnClosedByUser(EventArgs e)
        {
            ClosedByUser?.Invoke(this, e);
        }

        private void OnHyperlinkClicked(HyperLinkEventArgs e)
        {
            HyperlinkClicked?.Invoke(this, e);
        }

        private void ButtonViewClick(object sender, RoutedEventArgs e)
        {
            OnHyperlinkClicked(new HyperLinkEventArgs { HyperlinkObjectForRaisedEvent = HyperlinkObjectForRaisedEvent });
        }

        private void DoubleAnimationCompleted(object sender, EventArgs e)
        {
            if (!IsMouseOver)
            {
                Close();
            }
        }

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

        private void SetHyperLinkButton(string hyperlinkText)
        {
            if (!string.IsNullOrWhiteSpace(hyperlinkText))
            {
                ButtonView.Content = hyperlinkText;
                ButtonView.Visibility = Visibility.Visible;
            }
        }
    }
}