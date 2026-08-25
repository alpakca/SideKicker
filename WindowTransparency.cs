using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SideKicker
{
    internal static class WindowTransparency
    {
        private const int GWL_STYLE = -16;
        private const int GWLP_WNDPROC = -4;

        private const uint WS_POPUP = 0x80000000;
        private const uint WS_VISIBLE = 0x10000000;
        private const uint WS_CAPTION = 0x00C00000;
        private const uint WS_THICKFRAME = 0x00040000;
        private const uint WS_MINIMIZEBOX = 0x00020000;
        private const uint WS_MAXIMIZEBOX = 0x00010000;
        private const uint WS_SYSMENU = 0x00080000;
        private const uint WS_BORDER = 0x00800000;
        private const uint WS_DLGFRAME = 0x00400000;

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private const uint WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;
        private const int HTTRANSPARENT = -1;

        // ---- P/Invoke declarations (64-bit) ----
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X; public int Y; }

        public static void MakeBorderless(IntPtr hwnd)
        {
            uint style = (uint)GetWindowLongPtr(hwnd, GWL_STYLE).ToInt64();

            style &= ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX
                       | WS_SYSMENU | WS_BORDER | WS_DLGFRAME);
            style |= WS_POPUP | WS_VISIBLE;

            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        public static void EnableDwmPerPixelAlpha(IntPtr hwnd)
        {
            var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        public static void SetTopMost(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        // ---- Region-based click-through ----

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        // Kept alive as static fields: the delegate must not be GC'd while
        // Windows holds a native function pointer to it, and the original
        // window procedure is needed to forward every message we don't handle.
        private static WndProcDelegate _wndProcDelegate;
        private static IntPtr _originalWndProc = IntPtr.Zero;

        private static readonly object _rectLock = new object();
        private static List<Rectangle> _clickableRects = new List<Rectangle>();

        /// <summary>
        /// Installs a window-procedure hook so that clicks land only inside the
        /// given clickable rectangles (client/window coordinates — the same
        /// space you draw in with SpriteBatch); clicks anywhere else pass through
        /// to whatever window is behind this one. Call once, after MakeBorderless.
        /// </summary>
        public static void EnableRegionHitTesting(IntPtr hwnd, IEnumerable<Rectangle> initialClickableRects = null)
        {
            if (initialClickableRects != null)
                SetClickableRegions(initialClickableRects);

            _wndProcDelegate = CustomWndProc;
            IntPtr newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
            _originalWndProc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, newWndProcPtr);
        }

        /// <summary>
        /// Replaces the set of clickable rectangles. Safe to call every frame if
        /// your animation's clickable area moves or changes shape.
        /// </summary>
        public static void SetClickableRegions(IEnumerable<Rectangle> rects)
        {
            lock (_rectLock)
            {
                _clickableRects = new List<Rectangle>(rects);
            }
        }

        private static IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == WM_NCHITTEST)
            {
                long lp = lParam.ToInt64();
                short screenX = (short)(lp & 0xFFFF);
                short screenY = (short)((lp >> 16) & 0xFFFF);

                var pt = new POINT { X = screenX, Y = screenY };
                ScreenToClient(hWnd, ref pt);

                lock (_rectLock)
                {
                    foreach (var rect in _clickableRects)
                    {
                        if (rect.Contains(pt.X, pt.Y))
                        {
                            return new IntPtr(HTCLIENT);
                        }
                    }
                }

                return new IntPtr(HTTRANSPARENT);
            }

            return CallWindowProc(_originalWndProc, hWnd, msg, wParam, lParam);
        }

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
    }
}
