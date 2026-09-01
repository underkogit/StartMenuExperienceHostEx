using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace StartMenuExperienceHostEx.Services
{
    public sealed class KeyboardShortcutService : IDisposable
    {
        private readonly ILogger<KeyboardShortcutService>? _logger;
        private readonly KeyboardHook _keyboardHook;
        private readonly object _lock = new();

        private bool _disposed;

        public event EventHandler<ShortcutEventArgs>? ShortcutPressed;

        public KeyboardShortcutService(
            ILogger<KeyboardShortcutService>? logger = null)
        {
            _logger = logger;
            _keyboardHook = new KeyboardHook(logger);
            _keyboardHook.KeyEvent += OnKeyEvent;
        }

        public bool IsRunning => _keyboardHook.IsRunning;

        public void Start()
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_keyboardHook.IsRunning)
                    return;

                _keyboardHook.Start();

                _logger?.LogInformation(
                    "Keyboard shortcut service started");
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_disposed || !_keyboardHook.IsRunning)
                    return;

                _keyboardHook.Stop();

                _logger?.LogInformation(
                    "Keyboard shortcut service stopped");
            }
        }

        private void OnKeyEvent(
            uint virtualKeyCode,
            bool keyDown,
            bool altPressed,
            bool controlPressed,
            bool shiftPressed)
        {
            try
            {
                const uint VK_B = 0x42;

                if (!keyDown ||
                    virtualKeyCode != VK_B ||
                    !altPressed)
                {
                    return;
                }

                _logger?.LogDebug("ALT+B shortcut detected");

                var args = new ShortcutEventArgs
                {
                    VirtualKeyCode = virtualKeyCode,
                    IsAltPressed = altPressed,
                    IsControlPressed = controlPressed,
                    IsShiftPressed = shiftPressed,
                    Timestamp = DateTime.UtcNow
                };

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        ShortcutPressed?.Invoke(this, args);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(
                            ex,
                            "Error in shortcut event handler");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error processing keyboard event");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    nameof(KeyboardShortcutService));
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _keyboardHook.KeyEvent -= OnKeyEvent;
            _keyboardHook.Dispose();

            ShortcutPressed = null;

            GC.SuppressFinalize(this);
        }
    }

    public sealed class ShortcutEventArgs : EventArgs
    {
        public uint VirtualKeyCode { get; init; }

        public bool IsAltPressed { get; init; }

        public bool IsControlPressed { get; init; }

        public bool IsShiftPressed { get; init; }

        public DateTime Timestamp { get; init; }
    }

    internal sealed class KeyboardHook : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;

        private const uint WM_QUIT = 0x0012;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_SYSKEYDOWN = 0x0104;
        private const uint WM_SYSKEYUP = 0x0105;

        private const int VK_MENU = 0x12;
        private const int VK_CONTROL = 0x11;
        private const int VK_SHIFT = 0x10;

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        private readonly ILogger? _logger;
        private readonly object _lock = new();
        private readonly LowLevelKeyboardProc _proc;
        private readonly ManualResetEventSlim _threadReady = new(false);

        private Thread? _messageThread;
        private uint _messageThreadId;
        private IntPtr _hookId = IntPtr.Zero;

        private volatile bool _isRunning;
        private bool _disposed;

        public event Action<uint, bool, bool, bool, bool>? KeyEvent;

        public KeyboardHook(ILogger? logger = null)
        {
            _logger = logger;
            _proc = HookCallback;
        }

        public bool IsRunning => _isRunning;

        public void Start()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(
                        nameof(KeyboardHook));
                }

                if (_isRunning)
                    return;

                _threadReady.Reset();
                _isRunning = true;

                _messageThread = new Thread(MessageLoop)
                {
                    IsBackground = true,
                    Name = "KeyboardHookThread",
                    Priority = ThreadPriority.AboveNormal
                };

                _messageThread.Start();
            }

            _threadReady.Wait();
        }

        public void Stop()
        {
            Thread? thread;
            uint threadId;

            lock (_lock)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;
                thread = _messageThread;
                threadId = _messageThreadId;
            }

            if (threadId != 0)
            {
                if (!PostThreadMessage(
                        threadId,
                        WM_QUIT,
                        IntPtr.Zero,
                        IntPtr.Zero))
                {
                    int error = Marshal.GetLastWin32Error();

                    _logger?.LogWarning(
                        "Failed to post WM_QUIT. Win32 error: {ErrorCode}",
                        error);
                }
            }

            if (thread != null &&
                thread != Thread.CurrentThread &&
                thread.IsAlive)
            {
                if (!thread.Join(TimeSpan.FromSeconds(2)))
                {
                    _logger?.LogWarning(
                        "Keyboard hook thread did not stop within timeout");
                }
            }

            lock (_lock)
            {
                _messageThread = null;
                _messageThreadId = 0;
            }
        }

        private void MessageLoop()
        {
            _messageThreadId = GetCurrentThreadId();

            try
            {
                _hookId = SetHook(_proc);

                _logger?.LogInformation(
                    "Keyboard hook installed. Thread ID: {ThreadId}",
                    _messageThreadId);

                _threadReady.Set();

                while (true)
                {
                    int result = GetMessage(
                        out MSG message,
                        IntPtr.Zero,
                        0,
                        0);

                    if (result == -1)
                    {
                        int error = Marshal.GetLastWin32Error();

                        _logger?.LogError(
                            "GetMessage failed. Win32 error: {ErrorCode}",
                            error);

                        break;
                    }

                    if (result == 0)
                        break;

                    TranslateMessage(ref message);
                    DispatchMessage(ref message);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "Error in keyboard message loop");
            }
            finally
            {
                if (_hookId != IntPtr.Zero)
                {
                    if (!UnhookWindowsHookEx(_hookId))
                    {
                        int error = Marshal.GetLastWin32Error();

                        _logger?.LogWarning(
                            "Failed to unhook keyboard hook. " +
                            "Win32 error: {ErrorCode}",
                            error);
                    }

                    _hookId = IntPtr.Zero;
                }

                _threadReady.Set();

                _logger?.LogDebug(
                    "Keyboard hook thread exited");
            }
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using Process process = Process.GetCurrentProcess();
            using ProcessModule? module = process.MainModule;

            IntPtr moduleHandle = GetModuleHandle(
                module?.ModuleName);

            if (moduleHandle == IntPtr.Zero)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "Failed to get module handle");
            }

            IntPtr hookId = SetWindowsHookEx(
                WH_KEYBOARD_LL,
                proc,
                moduleHandle,
                0);

            if (hookId == IntPtr.Zero)
            {
                throw new Win32Exception(
                    Marshal.GetLastWin32Error(),
                    "Failed to install keyboard hook");
            }

            return hookId;
        }

        private IntPtr HookCallback(
            int nCode,
            IntPtr wParam,
            IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    var keyboardData =
                        Marshal.PtrToStructure<
                            KeyboardLowLevelHookStruct>(lParam);

                    bool keyDown =
                        wParam == (IntPtr)WM_KEYDOWN ||
                        wParam == (IntPtr)WM_SYSKEYDOWN;

                    bool keyUp =
                        wParam == (IntPtr)WM_KEYUP ||
                        wParam == (IntPtr)WM_SYSKEYUP;

                    if (keyDown || keyUp)
                    {
                        bool altPressed =
                            (GetAsyncKeyState(VK_MENU) & 0x8000) != 0;

                        bool controlPressed =
                            (GetAsyncKeyState(VK_CONTROL) & 0x8000) != 0;

                        bool shiftPressed =
                            (GetAsyncKeyState(VK_SHIFT) & 0x8000) != 0;

                        KeyEvent?.Invoke(
                            (uint)keyboardData.VirtualKeyCode,
                            keyDown,
                            altPressed,
                            controlPressed,
                            shiftPressed);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(
                        ex,
                        "Error in keyboard hook callback");
                }
            }

            return CallNextHookEx(
                _hookId,
                nCode,
                wParam,
                lParam);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            Stop();

            KeyEvent = null;
            _threadReady.Dispose();

            GC.SuppressFinalize(this);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardLowLevelHookStruct
        {
            public int VirtualKeyCode;
            public int ScanCode;
            public int Flags;
            public int Time;
            public IntPtr ExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr Hwnd;
            public uint Message;
            public IntPtr WParam;
            public IntPtr LParam;
            public uint Time;
            public POINT Point;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport(
            "user32.dll",
            SetLastError = true)]
        private static extern int GetMessage(
            out MSG lpMsg,
            IntPtr hWnd,
            uint wMsgFilterMin,
            uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(
            ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(
            ref MSG lpMsg);

        [DllImport(
            "user32.dll",
            SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc lpfn,
            IntPtr hMod,
            uint dwThreadId);

        [DllImport(
            "user32.dll",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(
            IntPtr hhk);

        [DllImport(
            "user32.dll",
            SetLastError = true)]
        private static extern IntPtr CallNextHookEx(
            IntPtr hhk,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport(
            "kernel32.dll",
            CharSet = CharSet.Unicode,
            SetLastError = true)]
        private static extern IntPtr GetModuleHandle(
            string? lpModuleName);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(
            int vKey);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport(
            "user32.dll",
            SetLastError = true)]
        private static extern bool PostThreadMessage(
            uint threadId,
            uint message,
            IntPtr wParam,
            IntPtr lParam);
    }
}
