#region Usings

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

#endregion

namespace SaveClicks.Presentation
{
    public static class WindowHelpers
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out NativeRect lpRect);

        [DllImport("user32.dll", EntryPoint = "ClientToScreen")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ClientToScreen([In] IntPtr hWnd, ref NativePoint lpPoint);

        /// <summary>
        ///     Gets the window inner bounds relative to screen pixels.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns></returns>
        private static NativeRect GetWindowInnerBounds(Window window)
        {
            var wndHandle = new WindowInteropHelper(window).Handle;

            var lpPoint = new NativePoint();
            ClientToScreen(wndHandle, ref lpPoint);

            NativeRect lpRect;
            GetClientRect(wndHandle, out lpRect);

            lpRect.Translate(lpPoint);

            return lpRect;
        }

        public static Rect GetMainWindowRect()
        {
            var mainWindow = Application.Current.MainWindow;
            switch (mainWindow.WindowState)
            {
                case WindowState.Normal:
                    return new Rect(mainWindow.Left, mainWindow.Top, mainWindow.ActualWidth, mainWindow.ActualHeight);
                case WindowState.Maximized:
                    return GetWindowInnerBounds(mainWindow).ToRect();
                default:
                    return Rect.Empty;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            private int Left;
            private int Top;
            private int Right;
            private int Bottom;

            public Rect ToRect()
            {
                return new Rect(Left, Top, Right - Left, Bottom - Top);
            }

            public void Translate(NativePoint point)
            {
                Left += point.x;
                Top += point.y;
                Right += point.x;
                Bottom += point.y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int x;
            public int y;
        }
    }
}