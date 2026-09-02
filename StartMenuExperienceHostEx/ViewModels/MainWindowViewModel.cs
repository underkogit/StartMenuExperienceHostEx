using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using ExperienceHost.DataAccess.SQL.Data;
using ExperienceHost.DataAccess.SQL.Entities;
using ExperienceHost.DataAccess.SQL.Structures;
using Microsoft.EntityFrameworkCore;
using StartMenuExperienceHostEx.Extentions;
using StartMenuExperienceHostEx.Helper;
using StartMenuExperienceHostEx.Services;
using StartMenuExperienceHostEx.Views;
using StartMenuExperienceHostEx.Views.Controls;

namespace StartMenuExperienceHostEx.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    [ObservableProperty] private ObservableCollection<ApplicationItemViewModel> _applications = new();

    private readonly SqliteDbContext _context;
    private readonly KeyboardShortcutService _shortcutService;
    private readonly WindowInputService _windowInputService;
    private DraggableCanvas _draggableCanvas;
    private MainWindow? _mainWindow;
    private bool _status = false;
    private bool _disposed;
    private CancellationTokenSource? _addApplicationCts;
    private IconExtractor _extractor = new IconExtractor(@"Native\win-x64\icon_extractor.exe");
    private WindowMessageInterceptor? _messageInterceptor;
    private nint HWND = 0;

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

    public async Task Refresh()
    {
        var apps = await _context.Applications.ToListAsync();


        _draggableCanvas.Children.Clear();
        foreach (var entityApplication in apps)
        {
            var item = new ApplicationControl
            {
                Id = entityApplication.Id,
                ApplicationName = entityApplication.Name,

                FilePath = entityApplication.FilePath,
                FullFilePath = entityApplication.FilePath,

                Image = File.Exists(entityApplication.ImagePath)
                    ? new Bitmap(entityApplication.ImagePath)
                    : null
            };

            Canvas.SetLeft(item, entityApplication.PositionX);
            Canvas.SetTop(item, entityApplication.PositionY);

            _draggableCanvas.Children.Add(item);
        }
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


            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(filePath);
            }
            catch
            {
                fullPath = filePath;
            }

            var position = _windowInputService.MousePositionGrid;
            var name = Path.GetFileNameWithoutExtension(fullPath);
            var disk = Path.GetPathRoot(fullPath) ?? string.Empty;

            var arguments = string.Empty;
            var imagePath = Path.GetFullPath($"icons/{name}.png");

            if (!_extractor.ExtractIcon(fullPath, imagePath))
                imagePath = string.Empty;

            var status = _context.AddApplication(
                name,
                fullPath,
                arguments,
                imagePath,
                tab,
                position);

            await Refresh();
            return status;
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
        if (_mainWindow?.DraggableCanvas is { } canvas)
        {
            _draggableCanvas = canvas;
            _draggableCanvas.ElementReleased += DraggableCanvasOnElementReleased;
        }

        HWND = _mainWindow?.GetWindowHandle() ?? 0;
        _messageInterceptor = new WindowMessageInterceptor(_mainWindow);
        _messageInterceptor.MessageReceived += OnMessageReceived;
        return this;
    }

    private void OnMessageReceived(object? sender, WindowMessageEventArgs e)
    {
        if (e.Message == 0x0007 || e.Message == 0x0008 || e.Message == 0x0006)
        {
            _mainWindow?.Dispatcher.Post(async () =>
            {
                if (_status)
                    _messageInterceptor?.MakeTopMost();
            });
            Console.WriteLine(e.ToString());
        }
    }

    private void DraggableCanvasOnElementReleased(ApplicationControl element, Point2D position)
    {
        _context.AppSetPoint(element.Id, position);
    }

    private void OnShortcutPressed(
        object? sender,
        ShortcutEventArgs e)
    {
        _mainWindow?.Dispatcher.Post(async () =>
        {
            if (_disposed || _mainWindow is null)
                return;

            _status = !_status;
            _mainWindow.SetWindowVisibility(_status);

            if (_status)
                await Refresh();

            WindowZOrder.SetWindowZOrder(HWND, 0);


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


        _messageInterceptor?.Dispose();
        _messageInterceptor = null;
    }
}