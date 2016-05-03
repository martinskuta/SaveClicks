#region Usings

using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using Application = System.Windows.Application;

#endregion

namespace SaveClicks.Presentation
{
    public static class WindowHelpers
    {
        private static Rect GetWindowInnerBounds(Window window)
        {
            var transformationMatrix = ScreenPixelsToWpfMatrix(window);
            if (transformationMatrix == null) return Rect.Empty;

            var screenToWpf = transformationMatrix.Value;
            var wndHandle = new WindowInteropHelper(window).Handle;
            var workArea = Screen.FromHandle(wndHandle).WorkingArea;

            //in screen pixels
            var topLeft = new Point(workArea.Left, workArea.Top);
            var bottomRight = new Point(workArea.Right, workArea.Bottom);

            //transformed to wpf
            return new Rect(screenToWpf.Transform(topLeft), screenToWpf.Transform(bottomRight));
        }

        private static Matrix? ScreenPixelsToWpfMatrix(Visual v)
        {
            return PresentationSource.FromVisual(v)?.CompositionTarget?.TransformFromDevice;
        }

        /// <summary>
        ///   Gets the rectange of window's client area in WPF coordinates (DPI aware)
        /// </summary>
        /// <remarks>
        ///   The methods help especially when the window is maximized, becuase the WPF window behavior when maximaized is that the
        ///   size and position is from before it was maximized. WPF coordinates are translated real pixel coordinates of the
        ///   screen to WPF coordinate system (DPI aware)
        /// </remarks>
        /// <returns></returns>
        public static Rect GetMainWindowRect()
        {
            var mainWindow = Application.Current.MainWindow;
            switch (mainWindow.WindowState)
            {
                case WindowState.Normal:
                    return new Rect(mainWindow.Left, mainWindow.Top, mainWindow.ActualWidth, mainWindow.ActualHeight);
                case WindowState.Maximized:
                    return GetWindowInnerBounds(mainWindow);
                default:
                    return Rect.Empty;
            }
        }
    }
}