using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace StartMenuExperienceHostEx.Services
{
    public class KeyboardMonitorService : IDisposable
    {
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        
        private readonly ILogger<KeyboardMonitorService> _logger;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;
        private bool _isRunning = false;
        private Thread _messageThread;
        
        public event Action<uint, bool> KeyEvent;
        
        public KeyboardMonitorService(ILogger<KeyboardMonitorService> logger = null)
        {
            _logger = logger;
            _proc = HookCallback;
        }
        
        public void Start()
        {
            if (_isRunning) return;
            
            _hookID = SetHook(_proc);
            _isRunning = true;
            
            _messageThread = new Thread(MessageLoop);
            _messageThread.IsBackground = true;
            _messageThread.Name = "KeyboardMonitorThread";
            _messageThread.Start();
            
            _logger?.LogInformation("Keyboard monitor started");
            Debug.WriteLine("Keyboard monitor started");
        }
        
        public void Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
            
            if (_messageThread != null && _messageThread.IsAlive)
            {
                _messageThread.Join(100);
            }
            
            _logger?.LogInformation("Keyboard monitor stopped");
            Debug.WriteLine("Keyboard monitor stopped");
        }
        
        public bool IsRunning => _isRunning;
        
        private void MessageLoop()
        {
            MSG msg;
            while (_isRunning && GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
        
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                try
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    bool keyDown = wParam == (IntPtr)WM_KEYDOWN;
                    
                    KeyEvent?.Invoke((uint)vkCode, keyDown);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error in keyboard hook callback");
                }
            }
            
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }
        
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        
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
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int x;
            public int y;
        }
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool TranslateMessage(ref MSG lpMsg);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);
        
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
        
        public void Dispose()
        {
            Stop();
            _proc = null;
            GC.SuppressFinalize(this);
        }
    }
}