using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ExperienceHost.DataAccess.SQL.Data;
using ExperienceHost.DataAccess.SQL.Entities;
using ExperienceHost.DataAccess.SQL.Structures;
using Microsoft.EntityFrameworkCore;
using StartMenuExperienceHostEx.Extentions;
using StartMenuExperienceHostEx.Helper;
using StartMenuExperienceHostEx.Services;
using StartMenuExperienceHostEx.Views;

namespace StartMenuExperienceHostEx.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private ObservableCollection<ApplicationViewModel> _applications = new();

    private readonly SqliteDbContext _context;
    private readonly KeyboardShortcutService _shortcutService;
    private readonly WindowInputService _windowInputService;

    private MainWindow? _mainWindow;
    private bool _status = true;
    private bool _disposed;
    private CancellationTokenSource? _addApplicationCts;
    public MainWindowViewModel(
        SqliteDbContext dbContext,
        KeyboardShortcutService shortcutService, WindowInputService windowInputService)
    {
        _windowInputService = windowInputService;

        _context = dbContext ??
                   throw new ArgumentNullException(nameof(dbContext));

        _shortcutService = shortcutService ??
                           throw new ArgumentNullException(
                               nameof(shortcutService));


        _shortcutService.ShortcutPressed += OnShortcutPressed;
        _shortcutService.Start();
    }


    public async Task<bool> AddApplicationAsync(
        string filePath,
        EntityTabItem? tab = null)
    {
        _addApplicationCts?.Cancel();
        _addApplicationCts?.Dispose();

        _addApplicationCts = new CancellationTokenSource();

        try
        {
            await Task.Delay(20, _addApplicationCts.Token);

            var position = _windowInputService.MousePositionGrid;

            return _context.AddApplication(
                filePath,
                tab,
                position);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }


    public MainWindowViewModel SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ??
                      throw new ArgumentNullException(nameof(mainWindow));

        _windowInputService.Attach(_mainWindow);
        return this;
    }

    private void OnShortcutPressed(
        object? sender,
        ShortcutEventArgs e)
    {
        _mainWindow?.Dispatcher.Post(() =>
        {
            if (_disposed || _mainWindow is null)
                return;

            _status = !_status;
            _mainWindow.SetWindowVisibility(_status);

            if (_mainWindow.GetWindowHandle() is { } handle && handle != IntPtr.Zero)
            {
                NativeLoader.NativeMethods.set_window_zorder((uint)handle, 0);
            }

            Console.WriteLine(
                $"{_status} ShortcutServiceOnShortcutPressed");
        });
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        _shortcutService.ShortcutPressed -= OnShortcutPressed;
        _shortcutService.Stop();
    }
}