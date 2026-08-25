using System;
using System.Runtime.InteropServices;

namespace SideKicker
{
    internal static class WindowTransparency
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        public static bool EnableDwmPerPixelAlpha(IntPtr hwnd)
        {
            var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            int hr = DwmExtendFrameIntoClientArea(hwnd, ref margins);
            return hr == 0; // S_OK
        }
    }
}
