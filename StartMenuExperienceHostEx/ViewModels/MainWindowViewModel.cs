using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using ExperienceHost.DataAccess.SQL.Data;
using ExperienceHost.DataAccess.SQL.Entities;
using ExperienceHost.DataAccess.SQL.Structures;
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

    private MainWindow? _mainWindow;
    private bool _status;
    private bool _disposed;

    public MainWindowViewModel(
        SqliteDbContext dbContext,
        KeyboardShortcutService shortcutService)
    {
        _context = dbContext ??
                   throw new ArgumentNullException(nameof(dbContext));

        _shortcutService = shortcutService ??
                           throw new ArgumentNullException(
                               nameof(shortcutService));

        _shortcutService.ShortcutPressed += OnShortcutPressed;
        _shortcutService.Start();
    }


    public void AddApplication(
        string filePath,
        EntityTabItem? tab = null,
        Point2D position = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return;
        }

        if (tab == null)
        {
            tab = _context.TabItems.FirstOrDefault();

            if (tab == null)
            {
                tab = new EntityTabItem
                {
                    Id = Guid.NewGuid(),
                    Name = "Test TAB",
                    Description = "TEST Description"
                };

                _context.TabItems.Add(tab);
            }
        }

        var fileInfo = new FileInfo(filePath);

        var application = new EntityApplication
        {
            Id = Guid.NewGuid(),
            Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
            Disk = Path.GetPathRoot(fileInfo.FullName) ?? string.Empty,
            FilePath = fileInfo.FullName,
            Arguments = string.Empty,
            ImagePath = string.Empty,
            Position = position,
            TabItemId = tab.Id,
            TabItem = tab
        };

        _context.Applications.Add(application);

        try
        {
            _context.SaveChanges();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }


    public MainWindowViewModel SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow ??
                      throw new ArgumentNullException(nameof(mainWindow));

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