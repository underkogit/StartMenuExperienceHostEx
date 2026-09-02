using System;
using System.Runtime.InteropServices;

namespace StartMenuExperienceHostEx.Helper;

public static class WindowZOrder
{
    private const int SWP_NOMOVE = 0x0002;
    private const int SWP_NOSIZE = 0x0001;
    private static readonly IntPtr HWND_TOP = IntPtr.Zero;
    private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);


    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags
    );


    public static int SetWindowZOrder(IntPtr hwnd, int action)
    {
        IntPtr insertAfter = action == 0 ? HWND_TOP : HWND_BOTTOM;
        bool result = SetWindowPos(hwnd, insertAfter, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        return result ? 1 : 0;
    }
}