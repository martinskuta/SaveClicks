#region using

using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace SaveClicks.System
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags = 0);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey([In] uint uCode, [In] uint uMapType = 0);
    }
}