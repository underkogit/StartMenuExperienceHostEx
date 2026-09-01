using System;
using Avalonia.Controls;

namespace StartMenuExperienceHostEx.Extentions;

public static class WinExtentions
{
    public static IntPtr GetWindowHandle(this Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (TopLevel.GetTopLevel(window)?.TryGetPlatformHandle() is { } handle)
        {
            return handle.Handle;
        }

        return IntPtr.Zero;
    }

    public static void SetWindowVisibility(
        this Window window,
        bool value)
    {
        ArgumentNullException.ThrowIfNull(window);

        if (value)
        {
            if (!window.IsVisible)
                window.Show();
        }
        else
        {
            if (window.IsVisible)
                window.Hide();
        }
    }
}