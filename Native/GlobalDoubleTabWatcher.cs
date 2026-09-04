using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

#nullable enable

namespace SideKicker.Native
{
    [SupportedOSPlatform("windows")]
    public static class GlobalDoubleTabWatcher
    {
        public static event Action? DoubleTapDetected;

        public static int DoubleTapThresholdMs { get; set; } = GetDoubleClickTime();

        public static bool IsRunning => _hookThreadId != 0;

        private const int VK_TAB = 0x09; // virtual key for Tab

        public static void Initialize()
        {
            if (_thread != null)
                return;

            _readySignal.Reset();
            _thread = new Thread(HookThreadProc)
            {
                IsBackground = true,
                Name = nameof(GlobalDoubleTabWatcher)
            };
            _thread.Start();

            // Block briefly until the hook is actually installed (or fails),
            // so callers know for sure what state things are in afterwards.
            _readySignal.Wait();

            if (_hookHandle == IntPtr.Zero)
            {
                _thread = null;
                throw new InvalidOperationException(
                    $"Failed to install the global Tab keyboard hook (Win32 error {_lastError}).");
            }
        }

        public static void Dispose()
        {
            uint threadId = _hookThreadId;
            if (threadId == 0)
                return;

            PostThreadMessage(threadId, WM_QUIT, UIntPtr.Zero, IntPtr.Zero);
            _thread?.Join(1000);
            _thread = null;
        }

        public static void Update()
        {
            int count = Interlocked.Exchange(ref _pendingDoubleTaps, 0);
            for (int i = 0; i < count; i++)
                DoubleTapDetected?.Invoke();
        }

        // ---------------------------------------------------------------
        // Win32 interop
        // ---------------------------------------------------------------

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_QUIT = 0x0012;
        private const uint LLKHF_ALTDOWN = 0x20;

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern int GetDoubleClickTime();

        [DllImport("user32.dll")]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern bool PostThreadMessage(uint idThread, uint msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        // Keep a strong reference so the GC never collects the delegate
        // while it's registered with SetWindowsHookEx.
        private static readonly LowLevelKeyboardProc _proc = HookCallback;

        private static Thread? _thread;
        private static readonly ManualResetEventSlim _readySignal = new(false);
        private static uint _hookThreadId;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static int _lastError;

        private static bool _tabCurrentlyDown;
        private static uint _lastTapTime;
        private static int _pendingDoubleTaps; // written on hook thread, drained by Update()

        private static void HookThreadProc()
        {
            _hookThreadId = GetCurrentThreadId();
            _hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
            if (_hookHandle == IntPtr.Zero)
                _lastError = Marshal.GetLastWin32Error();

            _readySignal.Set();

            if (_hookHandle == IntPtr.Zero)
            {
                _hookThreadId = 0;
                return;
            }

            try
            {
                while (GetMessage(out MSG msg, IntPtr.Zero, 0, 0))
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
            }
            finally
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
                _hookThreadId = 0;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var data = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                if (data.vkCode == VK_TAB)
                {
                    int msg = wParam.ToInt32();
                    bool isDown = msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN;
                    bool isUp = msg == WM_KEYUP || msg == WM_SYSKEYUP;

                    if (isDown && !_tabCurrentlyDown)
                    {
                        _tabCurrentlyDown = true;
                        bool altHeld = (data.flags & LLKHF_ALTDOWN) != 0;

                        if (!altHeld)
                        {
                            uint now = data.time;
                            // unchecked subtraction correctly handles the ~49.7-day
                            // wraparound of the tick-count clock used here.
                            if (_lastTapTime != 0 && unchecked(now - _lastTapTime) <= (uint)DoubleTapThresholdMs)
                            {
                                _lastTapTime = 0; // reset so 3 rapid taps = one event, not two
                                Interlocked.Increment(ref _pendingDoubleTaps);
                            }
                            else
                            {
                                _lastTapTime = now;
                            }
                        }
                    }
                    else if (isUp)
                    {
                        _tabCurrentlyDown = false;
                    }
                }
            }

            // Never swallow the key - always pass it down the chain so the
            // focused window (ours or anyone else's) still receives it.
            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }
    }
}