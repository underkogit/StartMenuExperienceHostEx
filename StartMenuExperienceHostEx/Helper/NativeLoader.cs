using System;
using System.IO;
using System.Runtime.InteropServices;

namespace StartMenuExperienceHostEx.Helper;

public static class NativeLoader
{
    private const string DllName = "StartMenuExperienceHostExNative.dll";

    static NativeLoader()
    {
        var paths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Native"),
            AppDomain.CurrentDomain.BaseDirectory,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin")
        };

        foreach (var path in paths)
        {
            var dllPath = Path.Combine(path, DllName);
            if (File.Exists(dllPath))
            {
                NativeLibrary.Load(dllPath);
                return;
            }
        }

        throw new FileNotFoundException($"Native DLL '{DllName}' not found in any expected location.");
    }

    public static class NativeMethods
    {
        [DllImport("StartMenuExperienceHostExNative.dll")]
        public static extern Int32 set_window_zorder(uint hwnd, Int32 action);
        
        
        
    }
}