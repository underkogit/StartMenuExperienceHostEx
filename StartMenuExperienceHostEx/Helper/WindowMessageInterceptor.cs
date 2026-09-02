using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using StartMenuExperienceHostEx.Extentions;

namespace StartMenuExperienceHostEx.Helper;

public class WindowMessageInterceptor : IDisposable
{
    private readonly Window _window;
    private readonly IntPtr _hwnd;
    private readonly HookProc _hookProc;
    private IntPtr _hookId = IntPtr.Zero;
    private bool _disposed;

    private delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    public event EventHandler<WindowMessageEventArgs>? MessageReceived;

    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SETFOCUS = 0x0007;
    private const int WM_KILLFOCUS = 0x0008;
    private const int WM_SIZE = 0x0005;
    private const int WM_MOVE = 0x0003;
    private const int WM_CLOSE = 0x0010;
    private const int WM_ACTIVATE = 0x0006;
    private static readonly IntPtr HWND_TOPMOST = new(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new(-2);

    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_SHOWWINDOW = 0x0040;
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(
        int idHook,
        HookProc lpfn,
        IntPtr hMod,
        uint dwThreadId
    );
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);
    
    public void MakeTopMost()
    {
        if (_hwnd == IntPtr.Zero)
            return;

        SetWindowPos(
            _hwnd,
            HWND_TOPMOST,
            0,
            0,
            0,
            0,
            SWP_NOMOVE |
            SWP_NOSIZE |
            SWP_NOACTIVATE |
            SWP_SHOWWINDOW);
    }
    
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(
        IntPtr hhk,
        int nCode,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

    public WindowMessageInterceptor(Window window)
    {
        _window = window ?? throw new ArgumentNullException(nameof(window));

        if (window.GetWindowHandle() is { } handle)
        {
            _hwnd = handle;
        }
        else
        {
            throw new InvalidOperationException("Window handle not available");
        }

        _hookProc = HookCallback;
        InstallHook();
    }

    private void InstallHook()
    {
        uint threadId = GetWindowThreadProcessId(_hwnd, IntPtr.Zero);
        _hookId = SetWindowsHookEx(4, _hookProc, IntPtr.Zero, threadId);

        if (_hookId == IntPtr.Zero)
        {
            Debug.WriteLine($"Failed to set hook. Error: {Marshal.GetLastWin32Error()}");
        }
    }

    private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            try
            {
                CWPSTRUCT msg = Marshal.PtrToStructure<CWPSTRUCT>(lParam);

                var args = new WindowMessageEventArgs
                {
                    Message = msg.message,
                    WParam = msg.wParam,
                    LParam = msg.lParam
                };

                OnMessageReceived(args);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in hook callback: {ex.Message}");
            }
        }

        return CallNextHookEx(_hookId, code, wParam, lParam);
    }

    protected virtual void OnMessageReceived(WindowMessageEventArgs e)
    {
        MessageReceived?.Invoke(this, e);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CWPSTRUCT
    {
        public IntPtr lParam;
        public IntPtr wParam;
        public int message;
        public IntPtr hwnd;
    }

    public void Dispose()
    {
        if (_disposed) return;

        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

public class WindowMessageEventArgs : EventArgs
{
    public int Message { get; set; }
    public IntPtr WParam { get; set; }
    public IntPtr LParam { get; set; }

    public override string ToString()
    {
        return $"Message: 0x{Message:X4}, WParam: 0x{WParam:X8}, LParam: 0x{LParam:X8}";
    }
}